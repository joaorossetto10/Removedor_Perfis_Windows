using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Helpers;

public static class UserProfileSafetyHelper
{
    private const string UsersPathPrefix = @"C:\Users\";
    private const string WindowsPathPrefix = @"C:\Windows\";
    private const string ServiceProfilesPathPrefix = @"C:\Windows\ServiceProfiles\";

    private static readonly HashSet<string> ProtectedProfileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Administrator",
        "Administrador",
        "Public",
        "Default",
        "Default User",
        "All Users",
        "DefaultAppPool",
        "WDAGUtilityAccount",
        "CodexSandboxOffline"
    };

    private static readonly HashSet<string> SystemOrServiceProfileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "LocalService",
        "NetworkService",
        "systemprofile",
        "DefaultAppPool",
        "WDAGUtilityAccount",
        "ksnproxy",
        "himds"
    };

    private static readonly HashSet<string> ProtectedUserPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        @"C:\Users\Public",
        @"C:\Users\Default",
        @"C:\Users\Default User",
        @"C:\Users\All Users"
    };

    public static UserProfileInfo Classify(UserProfileInfo profile, string loggedOnProfileName)
    {
        var blockReason = GetBlockReason(profile, loggedOnProfileName);

        if (string.IsNullOrWhiteSpace(blockReason))
        {
            return WithSafety(profile, canRemove: true, blockReason: string.Empty, status: "Disponível para remoção");
        }

        return WithSafety(profile, canRemove: false, blockReason, status: blockReason);
    }

    public static UserProfileInfo WithDuplicateNameAttention(UserProfileInfo profile)
    {
        var observation = string.IsNullOrWhiteSpace(profile.Observation)
            ? "Nome duplicado"
            : $"{profile.Observation}; Nome duplicado";

        var status = profile.CanRemove
            ? "Disponível - nome duplicado"
            : profile.Status;

        return Copy(profile, profile.CanRemove, profile.BlockReason, status, observation);
    }

    public static bool IsUsersProfilePath(string localPath)
    {
        return NormalizePath(localPath).StartsWith(UsersPathPrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSystemOrServiceProfile(UserProfileInfo profile)
    {
        var localPath = NormalizePath(profile.LocalPath);
        var profileName = NormalizeProfileName(profile.UserName);

        return IsSystemOrServicePath(localPath)
            || SystemOrServiceProfileNames.Contains(profileName);
    }

    public static string GetRemovalBlockReason(UserProfileInfo profile, string loggedOnProfileName)
    {
        if (!profile.CanRemove)
        {
            return string.IsNullOrWhiteSpace(profile.BlockReason)
                ? "Perfil bloqueado por regra de segurança"
                : profile.BlockReason;
        }

        if (string.IsNullOrWhiteSpace(profile.Sid))
        {
            return "SID não informado";
        }

        return GetBlockReason(profile, loggedOnProfileName);
    }

    private static string GetBlockReason(UserProfileInfo profile, string loggedOnProfileName)
    {
        if (profile.IsSpecial)
        {
            return "Bloqueado: perfil especial";
        }

        var localPath = NormalizePath(profile.LocalPath);
        var profileName = NormalizeProfileName(profile.UserName);

        if (IsSystemOrServiceProfile(profile))
        {
            return "Bloqueado: perfil de sistema";
        }

        if (string.IsNullOrWhiteSpace(localPath) || !localPath.StartsWith(UsersPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "Bloqueado: caminho fora de C:\\Users";
        }

        if (IsLoggedOnUserProfile(profileName, loggedOnProfileName))
        {
            return "Bloqueado: usuário está logado";
        }

        if (profile.IsLoaded)
        {
            return "Bloqueado: perfil em uso";
        }

        if (ProtectedUserPaths.Contains(localPath) || ProtectedProfileNames.Contains(profileName))
        {
            return "Bloqueado: perfil protegido";
        }

        return string.Empty;
    }

    private static bool IsSystemOrServicePath(string localPath)
    {
        return localPath.StartsWith(ServiceProfilesPathPrefix, StringComparison.OrdinalIgnoreCase)
            || localPath.StartsWith(WindowsPathPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string localPath)
    {
        return localPath.Trim().TrimEnd('\\');
    }

    private static string NormalizeProfileName(string profileName)
    {
        return profileName.Trim();
    }

    private static UserProfileInfo WithSafety(UserProfileInfo profile, bool canRemove, string blockReason, string status)
    {
        return Copy(profile, canRemove, blockReason, status, profile.Observation);
    }

    private static bool IsLoggedOnUserProfile(string profileName, string loggedOnProfileName)
    {
        return !string.IsNullOrWhiteSpace(profileName)
            && !string.IsNullOrWhiteSpace(loggedOnProfileName)
            && string.Equals(profileName, loggedOnProfileName, StringComparison.OrdinalIgnoreCase);
    }

    private static UserProfileInfo Copy(
        UserProfileInfo profile,
        bool canRemove,
        string blockReason,
        string status,
        string observation)
    {
        return new UserProfileInfo
        {
            IsSelected = canRemove && profile.IsSelected,
            UserName = profile.UserName,
            Sid = profile.Sid,
            LocalPath = profile.LocalPath,
            LastUseTime = profile.LastUseTime,
            IsLoaded = profile.IsLoaded,
            IsSpecial = profile.IsSpecial,
            IsSystemOrServiceProfile = IsSystemOrServiceProfile(profile),
            IsHiddenByDefault = IsSystemOrServiceProfile(profile),
            CanRemove = canRemove,
            BlockReason = blockReason,
            Status = status,
            Observation = observation,
            SizeDisplay = profile.SizeDisplay,
            OperationStatus = profile.OperationStatus
        };
    }
}
