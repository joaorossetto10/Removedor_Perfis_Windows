using System.Management;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Services;

public sealed class UserProfileRemovalService
{
    private readonly LogService _logService;

    public UserProfileRemovalService(LogService logService)
    {
        _logService = logService;
    }

    public Task<IReadOnlyList<ProfileRemovalResult>> RemoveProfilesAsync(
        string computerName,
        IReadOnlyList<UserProfileInfo> profiles,
        AdminCredentialInfo? credential,
        IProgress<ProfileRemovalResult> progress,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () => RemoveProfiles(computerName, profiles, credential, progress, cancellationToken),
            cancellationToken);
    }

    private IReadOnlyList<ProfileRemovalResult> RemoveProfiles(
        string computerName,
        IReadOnlyList<UserProfileInfo> profiles,
        AdminCredentialInfo? credential,
        IProgress<ProfileRemovalResult> progress,
        CancellationToken cancellationToken)
    {
        var results = new List<ProfileRemovalResult>();

        _logService.AddInfo("Iniciando remoção segura de perfis locais.");
        _logService.AddInfo($"Computador alvo: {computerName}.");
        _logService.AddInfo($"{profiles.Count} perfil(is) selecionado(s) para remoção.");

        var scope = CreateScope(computerName, credential);
        scope.Connect();
        _logService.AddInfo("Conexão WMI estabelecida para remoção.");

        var loggedOnProfileName = GetLoggedOnProfileName(GetLoggedOnUserName(scope));

        foreach (var profile in profiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var canceledResult = CreateSkipped(profile, "Ignorado: operação cancelada antes deste perfil.");
                results.Add(canceledResult);
                progress.Report(canceledResult);
                continue;
            }

            _logService.AddInfo($"Preparando remoção do perfil {profile} ({profile.Sid}).");

            var localValidation = UserProfileSafetyHelper.GetRemovalBlockReason(profile, loggedOnProfileName);
            if (!string.IsNullOrWhiteSpace(localValidation))
            {
                var skippedResult = CreateSkipped(profile, $"Ignorado: {localValidation}.");
                results.Add(skippedResult);
                progress.Report(skippedResult);
                _logService.AddWarning($"{profile} ignorado: {localValidation}.");
                continue;
            }

            try
            {
                using var wmiProfile = FindProfileBySid(scope, profile.Sid);
                if (wmiProfile is null)
                {
                    var skippedResult = CreateSkipped(profile, "Ignorado: perfil não encontrado no WMI.");
                    results.Add(skippedResult);
                    progress.Report(skippedResult);
                    _logService.AddWarning($"{profile} ignorado: SID não encontrado no WMI.");
                    continue;
                }

                var refreshedProfile = BuildProfileInfoFromWmi(wmiProfile);
                var refreshedValidation = UserProfileSafetyHelper.GetRemovalBlockReason(refreshedProfile, loggedOnProfileName);
                if (!string.IsNullOrWhiteSpace(refreshedValidation))
                {
                    var skippedResult = CreateSkipped(profile, $"Ignorado: {refreshedValidation}.");
                    results.Add(skippedResult);
                    progress.Report(skippedResult);
                    _logService.AddWarning($"{profile} ignorado após revalidação WMI: {refreshedValidation}.");
                    continue;
                }

                progress.Report(new ProfileRemovalResult(profile, success: false, skipped: false, confirmed: true, "Removendo..."));
                _logService.AddInfo($"Removendo perfil {profile} via Win32_UserProfile.Delete().");
                wmiProfile.Delete();

                using var confirmation = FindProfileBySid(scope, profile.Sid);
                if (confirmation is null)
                {
                    var successResult = new ProfileRemovalResult(profile, success: true, skipped: false, confirmed: true, "Removido");
                    results.Add(successResult);
                    progress.Report(successResult);
                    _logService.AddInfo($"Remoção confirmada para {profile}.");
                }
                else
                {
                    var notConfirmedResult = new ProfileRemovalResult(profile, success: false, skipped: false, confirmed: false, "Não confirmado");
                    results.Add(notConfirmedResult);
                    progress.Report(notConfirmedResult);
                    _logService.AddWarning($"Remoção não confirmada para {profile}: SID ainda existe no WMI.");
                }
            }
            catch (Exception exception)
            {
                var message = WmiErrorHelper.GetFriendlyMessage(exception);
                var errorResult = new ProfileRemovalResult(profile, success: false, skipped: false, confirmed: false, "Erro ao remover", message);
                results.Add(errorResult);
                progress.Report(errorResult);
                _logService.AddError($"Falha ao remover {profile}: {message}");
            }
        }

        LogSummary(results);
        return results;
    }

    private void LogSummary(IReadOnlyList<ProfileRemovalResult> results)
    {
        var removed = results.Count(result => result.Success);
        var skipped = results.Count(result => result.Skipped);
        var errors = results.Count(result => !result.Success && !result.Skipped && result.Message == "Erro ao remover");
        var notConfirmed = results.Count(result => !result.Success && !result.Skipped && result.Message == "Não confirmado");

        _logService.AddInfo($"Resumo da remoção: removidos={removed}, ignorados={skipped}, erros={errors}, não confirmados={notConfirmed}.");
    }

    private static ProfileRemovalResult CreateSkipped(UserProfileInfo profile, string message)
    {
        return new ProfileRemovalResult(profile, success: false, skipped: true, confirmed: true, message);
    }

    private static ManagementObject? FindProfileBySid(ManagementScope scope, string sid)
    {
        var escapedSid = sid.Replace("\\", "\\\\").Replace("'", "\\'");
        var query = new ObjectQuery($"SELECT SID, LocalPath, Loaded, Special, LastUseTime FROM Win32_UserProfile WHERE SID = '{escapedSid}'");

        using var searcher = new ManagementObjectSearcher(scope, query);
        using var results = searcher.Get();

        foreach (ManagementObject profile in results)
        {
            return profile;
        }

        return null;
    }

    private static UserProfileInfo BuildProfileInfoFromWmi(ManagementBaseObject profile)
    {
        var localPath = GetString(profile, "LocalPath");
        return new UserProfileInfo
        {
            UserName = GetProfileName(localPath),
            Sid = GetString(profile, "SID"),
            LocalPath = localPath,
            IsLoaded = GetBoolean(profile, "Loaded"),
            IsSpecial = GetBoolean(profile, "Special"),
            LastUseTime = GetDateTime(profile, "LastUseTime"),
            CanRemove = true
        };
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
