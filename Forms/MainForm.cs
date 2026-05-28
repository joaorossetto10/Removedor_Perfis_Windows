using RemovedorPerfisWindows.Services;

namespace RemovedorPerfisWindows.Forms;

public partial class MainForm : Form
{
    private readonly LogService _logService = new();

    public MainForm()
    {
        InitializeComponent();

        _logService.EntryAdded += OnLogEntryAdded;
        _logService.AddInfo("Aplicativo iniciado. Nenhuma conexão WMI ou remoção de perfis foi implementada nesta etapa.");
    }

    private void OnLogEntryAdded(object? sender, string entry)
    {
        txtLogs.AppendText(entry + Environment.NewLine);
    }
}
