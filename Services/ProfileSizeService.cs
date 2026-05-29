using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Services;

public sealed class ProfileSizeService
{
    private static readonly TimeSpan DefaultProfileTimeout = TimeSpan.FromSeconds(60);

    private const int NoError = 0;
    private const int AccessDenied = 5;
    private const int BadNetName = 67;
    private const int NetworkPathNotFound = 53;
    private const int SessionCredentialConflict = 1219;
    private const int ResourceTypeDisk = 1;

    private readonly LogService _logService;

    public ProfileSizeService(LogService logService)
    {
        _logService = logService;
    }

    public Task CalculateSizesAsync(
        string computerName,
        IReadOnlyList<UserProfileInfo> profiles,
        AdminCredentialInfo? credential,
        IProgress<ProfileSizeResult> progress,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => CalculateSizes(computerName, profiles, credential, progress, cancellationToken),
            cancellationToken);
    }

    private void CalculateSizes(
        string computerName,
        IReadOnlyList<UserProfileInfo> profiles,
        AdminCredentialInfo? credential,
        IProgress<ProfileSizeResult> progress,
        CancellationToken cancellationToken)
    {
        var profilesToCalculate = profiles
            .Where(profile => profile.CanRemove && UserProfileSafetyHelper.IsUsersProfilePath(profile.LocalPath))
            .ToList();

        var ignoredProfiles = profiles
            .Where(profile => !profilesToCalculate.Contains(profile))
            .ToList();

        var summary = new SizeCalculationSummary
        {
            Ignored = ignoredProfiles.Count
        };

        foreach (var profile in ignoredProfiles)
        {
            profile.SizeDisplay = profile.CanRemove ? "Não calculado" : "Bloqueado";
            progress.Report(new ProfileSizeResult(profile, profile.SizeDisplay, bytes: null, ProfileSizeResultStatus.Ignored));
            _logService.AddInfo($"Cálculo de tamanho ignorado para {profile}: perfil bloqueado ou fora de C:\\Users.");
        }

        _logService.AddInfo($"Iniciando cálculo de tamanho para {profilesToCalculate.Count} perfil(is), sequencialmente.");

        if (profilesToCalculate.Count == 0)
        {
            _logService.AddInfo("Nenhum perfil elegível para cálculo de tamanho.");
            LogSummary(summary);
            return;
        }

        var sharePath = $@"\\{computerName}\C$";
        var connectedWithCredential = false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (credential?.HasCredential == true)
            {
                var connectionResult = ConnectToShare(sharePath, credential);
                connectedWithCredential = connectionResult == NoError;

                if (!IsSuccessfulConnectionResult(connectionResult))
                {
                    _logService.AddWarning($"Falha ao acessar C$: {GetConnectionErrorMessage(connectionResult)}.");
                    ReportShareAccessFailure(profilesToCalculate, progress, summary, connectionResult);
                    LogSummary(summary);
                    return;
                }
            }
            else if (!Directory.Exists($@"{sharePath}\Users"))
            {
                _logService.AddWarning("Sem acesso ao compartilhamento C$ usando o usuário atual.");
                ReportAll(profilesToCalculate, progress, summary, "Sem acesso ao C$", ProfileSizeResultStatus.AccessDenied);
                LogSummary(summary);
                return;
            }

            foreach (var profile in profilesToCalculate)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    MarkRemainingAsCanceled(profilesToCalculate, profile, progress, summary);
                    _logService.AddWarning("Cálculo de tamanho cancelado pelo usuário.");
                    break;
                }

                progress.Report(new ProfileSizeResult(profile, "Calculando...", bytes: null, ProfileSizeResultStatus.Ignored));
                _logService.AddInfo($"Calculando tamanho do perfil {profile}.");

                var profilePath = BuildRemoteProfilePath(computerName, profile);
                var result = CalculateProfileSizeWithTimeout(profile, profilePath, DefaultProfileTimeout, cancellationToken);
                progress.Report(result);
                AddToSummary(summary, result);

                switch (result.Status)
                {
                    case ProfileSizeResultStatus.Calculated:
                        _logService.AddInfo($"Tamanho calculado para {profile}: {result.DisplayText}.");
                        break;
                    case ProfileSizeResultStatus.Timeout:
                        _logService.AddWarning($"Tempo excedido ao calcular tamanho de {profile}.");
                        break;
                    case ProfileSizeResultStatus.Canceled:
                        _logService.AddWarning($"Cálculo cancelado durante o perfil {profile}.");
                        MarkProfilesAfter(profilesToCalculate, profile, progress, summary, "Cancelado", ProfileSizeResultStatus.Canceled);
                        _logService.AddWarning("Cálculo de tamanho cancelado pelo usuário.");
                        LogSummary(summary);
                        return;
                    default:
                        _logService.AddWarning($"Falha ao calcular tamanho de {profile}: {result.DisplayText}.");
                        break;
                }
            }

            LogSummary(summary);
        }
        catch (OperationCanceledException)
        {
            MarkAllNotFinishedAsCanceled(profilesToCalculate, progress, summary);
            _logService.AddWarning("Cálculo de tamanho cancelado pelo usuário.");
            LogSummary(summary);
        }
        finally
        {
            if (connectedWithCredential)
            {
                DisconnectShare(sharePath);
                _logService.AddInfo("Conexão temporária com C$ encerrada.");
            }
        }
    }

    private static string BuildRemoteProfilePath(string computerName, UserProfileInfo profile)
    {
        var profileFolderName = Path.GetFileName(profile.LocalPath.TrimEnd('\\'));
        return $@"\\{computerName}\C$\Users\{profileFolderName}";
    }

    private ProfileSizeResult CalculateProfileSizeWithTimeout(
        UserProfileInfo profile,
        string profilePath,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var calculationTask = Task.Run(
            () => CalculateProfileSize(profile, profilePath, timeout, timeoutCancellation.Token),
            timeoutCancellation.Token);

        try
        {
            if (calculationTask.Wait(timeout, cancellationToken))
            {
                return calculationTask.Result;
            }

            timeoutCancellation.Cancel();
            return new ProfileSizeResult(profile, "Tempo excedido", bytes: null, ProfileSizeResultStatus.Timeout);
        }
        catch (OperationCanceledException)
        {
            timeoutCancellation.Cancel();
            return new ProfileSizeResult(profile, "Cancelado", bytes: null, ProfileSizeResultStatus.Canceled);
        }
        catch (AggregateException exception) when (exception.InnerExceptions.All(inner => inner is OperationCanceledException))
        {
            return new ProfileSizeResult(profile, "Cancelado", bytes: null, ProfileSizeResultStatus.Canceled);
        }
        catch (AggregateException)
        {
            return new ProfileSizeResult(profile, "Erro ao calcular", bytes: null, ProfileSizeResultStatus.Error);
        }
    }

    private ProfileSizeResult CalculateProfileSize(
        UserProfileInfo profile,
        string profilePath,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(profilePath))
            {
                return new ProfileSizeResult(profile, "Sem acesso ao C$", bytes: null, ProfileSizeResultStatus.AccessDenied);
            }

            var stopwatch = Stopwatch.StartNew();
            var result = GetDirectorySize(profilePath, stopwatch, timeout, cancellationToken);

            if (result.TimedOut)
            {
                return new ProfileSizeResult(profile, "Tempo excedido", bytes: null, ProfileSizeResultStatus.Timeout);
            }

            if (result.Canceled)
            {
                return new ProfileSizeResult(profile, "Cancelado", bytes: null, ProfileSizeResultStatus.Canceled);
            }

            if (result.HasErrors)
            {
                return new ProfileSizeResult(profile, "Erro ao calcular", bytes: null, ProfileSizeResultStatus.Error);
            }

            return new ProfileSizeResult(profile, FormatSize(result.Bytes), result.Bytes, ProfileSizeResultStatus.Calculated);
        }
        catch (OperationCanceledException)
        {
            return new ProfileSizeResult(profile, "Cancelado", bytes: null, ProfileSizeResultStatus.Canceled);
        }
        catch (UnauthorizedAccessException)
        {
            return new ProfileSizeResult(profile, "Sem acesso ao C$", bytes: null, ProfileSizeResultStatus.AccessDenied);
        }
        catch (IOException)
        {
            return new ProfileSizeResult(profile, "Erro ao calcular", bytes: null, ProfileSizeResultStatus.Error);
        }
        catch (System.Security.SecurityException)
        {
            return new ProfileSizeResult(profile, "Sem acesso ao C$", bytes: null, ProfileSizeResultStatus.AccessDenied);
        }
    }

    private static DirectorySizeInfo GetDirectorySize(
        string rootPath,
        Stopwatch stopwatch,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        long totalBytes = 0;
        var hasErrors = false;
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(rootPath);

        var options = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        while (pendingDirectories.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new DirectorySizeInfo(totalBytes, hasErrors, TimedOut: false, Canceled: true);
            }

            if (stopwatch.Elapsed >= timeout)
            {
                return new DirectorySizeInfo(totalBytes, hasErrors, TimedOut: true, Canceled: false);
            }

            var currentDirectory = pendingDirectories.Pop();

            try
            {
                var directoryInfo = new DirectoryInfo(currentDirectory);
                if (directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    continue;
                }

                foreach (var fileInfo in directoryInfo.EnumerateFiles("*", options))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new DirectorySizeInfo(totalBytes, hasErrors, TimedOut: false, Canceled: true);
                    }

                    if (stopwatch.Elapsed >= timeout)
                    {
                        return new DirectorySizeInfo(totalBytes, hasErrors, TimedOut: true, Canceled: false);
                    }

                    try
                    {
                        if (!fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            totalBytes += fileInfo.Length;
                        }
                    }
                    catch (Exception exception) when (IsNonFatalFileSystemException(exception))
                    {
                        hasErrors = true;
                    }
                }

                foreach (var childDirectory in directoryInfo.EnumerateDirectories("*", options))
                {
                    if (!childDirectory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        pendingDirectories.Push(childDirectory.FullName);
                    }
                }
            }
            catch (Exception exception) when (IsNonFatalFileSystemException(exception))
            {
                hasErrors = true;
            }
        }

        return new DirectorySizeInfo(totalBytes, hasErrors, TimedOut: false, Canceled: false);
    }

    private static bool IsNonFatalFileSystemException(Exception exception)
    {
        return exception is UnauthorizedAccessException
            or IOException
            or DirectoryNotFoundException
            or PathTooLongException
            or System.Security.SecurityException;
    }

    private static string FormatSize(long bytes)
    {
        var megabytes = bytes / 1024d / 1024d;

        if (megabytes < 1024d)
        {
            return $"{Math.Round(megabytes):N0} MB";
        }

        var gigabytes = megabytes / 1024d;
        return $"{gigabytes:N2} GB";
    }

    private void ReportShareAccessFailure(
        IReadOnlyList<UserProfileInfo> profiles,
        IProgress<ProfileSizeResult> progress,
        SizeCalculationSummary summary,
        int connectionResult)
    {
        var status = connectionResult switch
        {
            AccessDenied or NetworkPathNotFound or BadNetName => ProfileSizeResultStatus.AccessDenied,
            _ => ProfileSizeResultStatus.Error
        };

        var message = status == ProfileSizeResultStatus.AccessDenied ? "Sem acesso ao C$" : "Erro ao calcular";
        ReportAll(profiles, progress, summary, message, status);
    }

    private static void ReportAll(
        IReadOnlyList<UserProfileInfo> profiles,
        IProgress<ProfileSizeResult> progress,
        SizeCalculationSummary summary,
        string displayText,
        ProfileSizeResultStatus status)
    {
        foreach (var profile in profiles)
        {
            var result = new ProfileSizeResult(profile, displayText, bytes: null, status);
            progress.Report(result);
            AddToSummary(summary, result);
        }
    }

    private static void MarkRemainingAsCanceled(
        IReadOnlyList<UserProfileInfo> profiles,
        UserProfileInfo currentProfile,
        IProgress<ProfileSizeResult> progress,
        SizeCalculationSummary summary)
    {
        MarkProfilesAfter(profiles, currentProfile, progress, summary, "Cancelado", ProfileSizeResultStatus.Canceled);
    }

    private static void MarkAllNotFinishedAsCanceled(
        IReadOnlyList<UserProfileInfo> profiles,
        IProgress<ProfileSizeResult> progress,
        SizeCalculationSummary summary)
    {
        foreach (var profile in profiles.Where(profile => profile.SizeDisplay is "Não calculado" or "Calculando..."))
        {
            var result = new ProfileSizeResult(profile, "Cancelado", bytes: null, ProfileSizeResultStatus.Canceled);
            progress.Report(result);
            AddToSummary(summary, result);
        }
    }

    private static void MarkProfilesAfter(
        IReadOnlyList<UserProfileInfo> profiles,
        UserProfileInfo currentProfile,
        IProgress<ProfileSizeResult> progress,
        SizeCalculationSummary summary,
        string displayText,
        ProfileSizeResultStatus status)
    {
        var startIndex = -1;
        for (var index = 0; index < profiles.Count; index++)
        {
            if (ReferenceEquals(profiles[index], currentProfile))
            {
                startIndex = index;
                break;
            }
        }
        if (startIndex < 0)
        {
            return;
        }

        for (var index = startIndex + 1; index < profiles.Count; index++)
        {
            var profile = profiles[index];
            if (profile.SizeDisplay is not ("Não calculado" or "Calculando..."))
            {
                continue;
            }

            var result = new ProfileSizeResult(profile, displayText, bytes: null, status);
            progress.Report(result);
            AddToSummary(summary, result);
        }
    }

    private static void AddToSummary(SizeCalculationSummary summary, ProfileSizeResult result)
    {
        switch (result.Status)
        {
            case ProfileSizeResultStatus.Calculated:
                summary.Calculated++;
                break;
            case ProfileSizeResultStatus.Ignored:
                break;
            case ProfileSizeResultStatus.AccessDenied:
                summary.Errors++;
                break;
            case ProfileSizeResultStatus.Timeout:
                summary.Timeouts++;
                break;
            case ProfileSizeResultStatus.Canceled:
                summary.Canceled++;
                break;
            case ProfileSizeResultStatus.Error:
                summary.Errors++;
                break;
        }
    }

    private void LogSummary(SizeCalculationSummary summary)
    {
        _logService.AddInfo(
            $"Resumo do cálculo: calculados={summary.Calculated}, ignorados={summary.Ignored}, cancelados={summary.Canceled}, erros={summary.Errors}, timeouts={summary.Timeouts}.");
    }

    private static bool IsSuccessfulConnectionResult(int result)
    {
        return result is NoError or SessionCredentialConflict;
    }

    private static string GetConnectionErrorMessage(int result)
    {
        return result switch
        {
            AccessDenied => "acesso negado ao compartilhamento administrativo",
            NetworkPathNotFound => "computador ou caminho de rede inacessível",
            BadNetName => "compartilhamento administrativo indisponível",
            SessionCredentialConflict => "já existe uma conexão com credenciais diferentes",
            _ => new Win32Exception(result).Message
        };
    }

    private static int ConnectToShare(string sharePath, AdminCredentialInfo credential)
    {
        var netResource = new NetResource
        {
            Scope = 0,
            Type = ResourceTypeDisk,
            DisplayType = 0,
            Usage = 0,
            RemoteName = sharePath,
            LocalName = null,
            Comment = null,
            Provider = null
        };

        return WNetAddConnection2(netResource, credential.Password, credential.UserName, 0);
    }

    private static void DisconnectShare(string sharePath)
    {
        WNetCancelConnection2(sharePath, 0, force: false);
    }

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetAddConnection2(NetResource netResource, string password, string userName, int flags);

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetCancelConnection2(string name, int flags, bool force);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class NetResource
    {
        public int Scope;
        public int Type;
        public int DisplayType;
        public int Usage;
        public string? LocalName;
        public string? RemoteName;
        public string? Comment;
        public string? Provider;
    }

    private sealed record DirectorySizeInfo(long Bytes, bool HasErrors, bool TimedOut, bool Canceled);

    private sealed class SizeCalculationSummary
    {
        public int Calculated { get; set; }
        public int Ignored { get; set; }
        public int Canceled { get; set; }
        public int Errors { get; set; }
        public int Timeouts { get; set; }
    }
}
