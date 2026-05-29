namespace RemovedorPerfisWindows.Models;

public sealed class ProfileRemovalResult
{
    public ProfileRemovalResult(
        UserProfileInfo profile,
        bool success,
        bool skipped,
        bool confirmed,
        string message,
        string errorMessage = "")
    {
        Profile = profile;
        ProfileName = profile.UserName;
        Sid = profile.Sid;
        LocalPath = profile.LocalPath;
        Success = success;
        Skipped = skipped;
        Confirmed = confirmed;
        Message = message;
        ErrorMessage = errorMessage;
    }

    public UserProfileInfo Profile { get; }
    public string ProfileName { get; }
    public string Sid { get; }
    public string LocalPath { get; }
    public bool Success { get; }
    public bool Skipped { get; }
    public bool Confirmed { get; }
    public string Message { get; }
    public string ErrorMessage { get; }
}
