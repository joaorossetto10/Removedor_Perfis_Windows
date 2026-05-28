using System.Management;
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

        var query = new ObjectQuery("SELECT SID, LocalPath, Loaded, Special, LastUseTime FROM Win32_UserProfile");

        using var searcher = new ManagementObjectSearcher(scope, query);
        using var results = searcher.Get();

        foreach (ManagementObject profile in results)
        {
            using (profile)
            {
                var isSpecial = GetBoolean(profile, "Special");
                if (isSpecial)
                {
                    _logService.AddInfo($"Perfil especial ignorado: {GetString(profile, "SID")}.");
                    continue;
                }

                var localPath = GetString(profile, "LocalPath");
                var isLoaded = GetBoolean(profile, "Loaded");

                profiles.Add(new UserProfileInfo
                {
                    UserName = GetProfileName(localPath),
                    Sid = GetString(profile, "SID"),
                    LocalPath = localPath,
                    IsLoaded = isLoaded,
                    IsSpecial = isSpecial,
                    LastUseTime = GetDateTime(profile, "LastUseTime"),
                    Status = isLoaded ? "Em uso" : "Disponível para análise"
                });
            }
        }

        _logService.AddInfo($"{profiles.Count} perfil(is) local(is) carregado(s) de {computerName}.");
        return profiles
            .OrderBy(profile => profile.UserName)
            .ThenBy(profile => profile.Sid)
            .ToList();
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
