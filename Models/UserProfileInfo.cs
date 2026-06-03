using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemovedorPerfisWindows.Models;

public sealed class UserProfileInfo : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _sizeDisplay = "Não calculado";
    private long? _sizeBytes;
    private string _operationStatus = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public string UserName { get; init; } = string.Empty;
    public string Sid { get; init; } = string.Empty;
    public string LocalPath { get; init; } = string.Empty;
    public DateTime? LastUseTime { get; init; }
    public bool IsLoaded { get; init; }
    public bool IsSpecial { get; init; }
    public bool IsSystemOrServiceProfile { get; init; }
    public bool IsHiddenByDefault { get; init; }
    public bool CanRemove { get; init; }
    public string BlockReason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Observation { get; init; } = string.Empty;

    public string SizeDisplay
    {
        get => _sizeDisplay;
        set => SetField(ref _sizeDisplay, value);
    }

    public long? SizeBytes
    {
        get => _sizeBytes;
        set => SetField(ref _sizeBytes, value);
    }

    public string OperationStatus
    {
        get => _operationStatus;
        set => SetField(ref _operationStatus, value);
    }

    public string LastUseTimeText => LastUseTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Não informado";

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(UserName) ? Sid : UserName;
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
