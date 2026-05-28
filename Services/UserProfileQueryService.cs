using System.Management;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Services;

public sealed class UserProfileQueryService
{
    private readonly LogService _logService;

    public UserProfileQueryService(LogService logService)
    {
        _logService = logService;
    }

    public Task<IReadOnlyList<UserProfileInfo>> GetProfilesAsync(string computerName, AdminCredentialInfo? credential = null)
    {
        return Task.Run(() => GetProfiles(computerName, credential));
    }

    private IReadOnlyList<UserProfileInfo> GetProfiles(string computerName, AdminCredentialInfo? credential)
    {
        _logService.AddInfo($"Iniciando consulta WMI em {computerName}.");
        _logService.AddInfo(credential?.HasCredential == true
            ? $"Consulta usando credencial informada para o usuário {credential.UserName}."
            : "Consulta usando o usuário atual do Windows.");

        var profiles = new List<UserProfileInfo>();
        var scope = CreateScope(computerName, credential);
        scope.Connect();
        _logService.AddInfo("Conexão WMI estabelecida com sucesso.");

        var loggedOnUserName = GetLoggedOnUserName(scope);
        var loggedOnProfileName = GetLoggedOnProfileName(loggedOnUserName);

        if (string.IsNullOrWhiteSpace(loggedOnUserName))
        {
            _logService.AddInfo("Nenhum usuário interativo logado detectado.");
        }
        else
        {
            _logService.AddInfo($"Usuário interativo logado detectado: {loggedOnUserName}.");
        }

        var query = new ObjectQuery("SELECT SID, LocalPath, Loaded, Special, LastUseTime FROM Win32_UserProfile");

        using var searcher = new ManagementObjectSearcher(scope, query);
        using var results = searcher.Get();

        var totalProfilesFound = 0;

        foreach (ManagementObject profile in results)
        {
            using (profile)
            {
                totalProfilesFound++;
                var isSpecial = GetBoolean(profile, "Special");
                var localPath = GetString(profile, "LocalPath");
                var isLoaded = GetBoolean(profile, "Loaded");

                var profileInfo = new UserProfileInfo
                {
                    UserName = GetProfileName(localPath),
                    Sid = GetString(profile, "SID"),
                    LocalPath = localPath,
                    IsLoaded = isLoaded,
                    IsSpecial = isSpecial,
                    LastUseTime = GetDateTime(profile, "LastUseTime")
                };

                profiles.Add(UserProfileSafetyHelper.Classify(profileInfo, loggedOnProfileName));
            }
        }

        var profilesWithDuplicateAttention = MarkDuplicateProfileNames(profiles);

        var orderedProfiles = profilesWithDuplicateAttention
            .OrderBy(profile => profile.UserName)
            .ThenBy(profile => profile.Sid)
            .ToList();

        LogClassificationSummary(computerName, totalProfilesFound, orderedProfiles);
        return orderedProfiles;
    }

    private void LogClassificationSummary(string computerName, int totalProfilesFound, IReadOnlyList<UserProfileInfo> profiles)
    {
        var removableCount = profiles.Count(profile => profile.CanRemove);
        var blockedProfiles = profiles.Where(profile => !profile.CanRemove).ToList();

        _logService.AddInfo($"{totalProfilesFound} perfil(is) retornado(s) pelo WMI em {computerName}.");
        _logService.AddInfo($"{removableCount} perfil(is) disponível(is) para análise.");
        _logService.AddInfo($"{blockedProfiles.Count} perfil(is) bloqueado(s) por regra de segurança.");

        foreach (var group in blockedProfiles.GroupBy(profile => profile.BlockReason).OrderBy(group => group.Key))
        {
            _logService.AddInfo($"{group.Count()} bloqueado(s): {group.Key}.");
        }

        var duplicatedNames = profiles
            .Where(profile => string.Equals(profile.Observation, "Nome duplicado", StringComparison.OrdinalIgnoreCase)
                || profile.Observation.Contains("Nome duplicado", StringComparison.OrdinalIgnoreCase))
            .Select(profile => profile.UserName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        if (duplicatedNames.Count > 0)
        {
            _logService.AddWarning($"Atenção: foram encontrados perfis com nomes duplicados: {string.Join(", ", duplicatedNames)}.");
        }
        else
        {
            _logService.AddInfo("Nenhum nome de perfil duplicado foi encontrado em C:\\Users.");
        }
    }

    private static IReadOnlyList<UserProfileInfo> MarkDuplicateProfileNames(IReadOnlyList<UserProfileInfo> profiles)
    {
        var duplicatedNames = profiles
            .Where(profile => UserProfileSafetyHelper.IsUsersProfilePath(profile.LocalPath))
            .GroupBy(profile => profile.UserName, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (duplicatedNames.Count == 0)
        {
            return profiles;
        }

        return profiles
            .Select(profile => duplicatedNames.Contains(profile.UserName)
                ? UserProfileSafetyHelper.WithDuplicateNameAttention(profile)
                : profile)
            .ToList();
    }

    private static string GetLoggedOnUserName(ManagementScope scope)
    {
        var query = new ObjectQuery("SELECT UserName FROM Win32_ComputerSystem");

        using var searcher = new ManagementObjectSearcher(scope, query);
        using var results = searcher.Get();

        foreach (ManagementObject computerSystem in results)
        {
            using (computerSystem)
            {
                return GetString(computerSystem, "UserName");
            }
        }

        return string.Empty;
    }

    private static string GetLoggedOnProfileName(string loggedOnUserName)
    {
        if (string.IsNullOrWhiteSpace(loggedOnUserName))
        {
            return string.Empty;
        }

        var trimmedUserName = loggedOnUserName.Trim();
        var slashIndex = trimmedUserName.LastIndexOf('\\');

        if (slashIndex >= 0 && slashIndex < trimmedUserName.Length - 1)
        {
            return trimmedUserName[(slashIndex + 1)..];
        }

        var atIndex = trimmedUserName.IndexOf('@');

        return atIndex > 0 ? trimmedUserName[..atIndex] : trimmedUserName;
    }

    private static ManagementScope CreateScope(string computerName, AdminCredentialInfo? credential)
    {
        var options = new ConnectionOptions
        {
            EnablePrivileges = true,
            Impersonation = ImpersonationLevel.Impersonate,
            Authentication = AuthenticationLevel.PacketPrivacy
        };

        if (credential?.HasCredential == true)
        {
            options.Username = credential.UserName;
            options.Password = credential.Password;
        }

        return new ManagementScope($@"\\{computerName}\root\cimv2", options);
    }

    private static bool GetBoolean(ManagementBaseObject profile, string propertyName)
    {
        return profile[propertyName] is bool value && value;
    }

    private static string GetString(ManagementBaseObject profile, string propertyName)
    {
        return profile[propertyName]?.ToString() ?? string.Empty;
    }

    private static DateTime? GetDateTime(ManagementBaseObject profile, string propertyName)
    {
        var value = GetString(profile, propertyName);
        return string.IsNullOrWhiteSpace(value) ? null : ManagementDateTimeConverter.ToDateTime(value);
    }

    private static string GetProfileName(string localPath)
    {
        if (string.IsNullOrWhiteSpace(localPath))
        {
            return "Não informado";
        }

        var trimmedPath = localPath.TrimEnd('\\');
        var lastSeparatorIndex = trimmedPath.LastIndexOf('\\');

        return lastSeparatorIndex >= 0 ? trimmedPath[(lastSeparatorIndex + 1)..] : trimmedPath;
    }
}
