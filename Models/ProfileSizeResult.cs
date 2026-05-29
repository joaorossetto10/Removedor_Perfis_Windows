namespace RemovedorPerfisWindows.Models;

public sealed class ProfileSizeResult
{
    public ProfileSizeResult(UserProfileInfo profile, string displayText, long? bytes, ProfileSizeResultStatus status)
    {
        Profile = profile;
        DisplayText = displayText;
        Bytes = bytes;
        Status = status;
    }

    public UserProfileInfo Profile { get; }
    public string DisplayText { get; }
    public long? Bytes { get; }
    public ProfileSizeResultStatus Status { get; }
    public bool Success => Status == ProfileSizeResultStatus.Calculated;
}

public enum ProfileSizeResultStatus
{
    Calculated,
    Ignored,
    AccessDenied,
    Timeout,
    Canceled,
    Error
}
