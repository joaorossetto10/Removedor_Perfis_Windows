namespace RemovedorPerfisWindows.Services;

public sealed class LogService
{
    private readonly List<string> _entries = [];

    public event EventHandler<string>? EntryAdded;

    public IReadOnlyList<string> Entries => _entries;

    public void AddInfo(string message)
    {
        Add("INFO", message);
    }

    public void AddWarning(string message)
    {
        Add("AVISO", message);
    }

    public void AddError(string message)
    {
        Add("ERRO", message);
    }

    private void Add(string level, string message)
    {
        var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        _entries.Add(entry);
        EntryAdded?.Invoke(this, entry);
    }
}
