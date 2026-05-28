namespace RemovedorPerfisWindows.Models;

public sealed class UserProfileInfo
{
    public string UserName { get; init; } = string.Empty;
    public string Sid { get; init; } = string.Empty;
    public string LocalPath { get; init; } = string.Empty;
    public DateTime? LastUseTime { get; init; }
    public bool IsLoaded { get; init; }
    public bool IsSpecial { get; init; }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(UserName) ? Sid : UserName;
    }
}
