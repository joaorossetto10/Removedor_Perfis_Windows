namespace RemovedorPerfisWindows.Models;

public sealed class UserProfileInfo
{
    public bool IsSelected { get; set; }
    public string UserName { get; init; } = string.Empty;
    public string Sid { get; init; } = string.Empty;
    public string LocalPath { get; init; } = string.Empty;
    public DateTime? LastUseTime { get; init; }
    public bool IsLoaded { get; init; }
    public bool IsSpecial { get; init; }
    public bool CanRemove { get; init; }
    public string BlockReason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Observation { get; init; } = string.Empty;

    public string LastUseTimeText => LastUseTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Não informado";

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(UserName) ? Sid : UserName;
    }
}
