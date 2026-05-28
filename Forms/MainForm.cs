using System.ComponentModel;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;
using RemovedorPerfisWindows.Services;

namespace RemovedorPerfisWindows.Forms;

public partial class MainForm : Form
{
    private readonly LogService _logService = new();
    private readonly BindingList<UserProfileInfo> _profiles = [];
    private UserProfileQueryService _userProfileQueryService = null!;

    public MainForm()
    {
        InitializeComponent();

        _userProfileQueryService = new UserProfileQueryService(_logService);
        dgvProfiles.AutoGenerateColumns = false;
        dgvProfiles.DataSource = _profiles;

        _logService.EntryAdded += OnLogEntryAdded;
        _logService.AddInfo("Aplicativo iniciado. A etapa atual permite apenas listar perfis locais via Win32_UserProfile.");
    }

    private void OnLogEntryAdded(object? sender, string entry)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => OnLogEntryAdded(sender, entry));
            return;
        }

        txtLogs.AppendText(entry + Environment.NewLine);
    }

    private async void BtnLoadProfiles_Click(object? sender, EventArgs e)
    {
        var computerName = txtComputerName.Text.Trim();

        if (string.IsNullOrWhiteSpace(computerName))
        {
            _logService.AddWarning("Informe o nome do computador remoto antes de carregar os perfis.");
            MessageBox.Show(
                "Informe o nome do computador remoto.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            txtComputerName.Focus();
            return;
        }

        SetLoadingState(true);
        _profiles.Clear();
        _logService.AddInfo($"Solicitação de carregamento de perfis para {computerName}.");

        try
        {
            var profiles = await _userProfileQueryService.GetProfilesAsync(computerName);

            foreach (var profile in profiles)
            {
                _profiles.Add(profile);
            }

            lblStatus.Text = $"{profiles.Count} perfil(is) local(is) encontrado(s).";
            _logService.AddInfo("Consulta concluída com sucesso.");
        }
        catch (Exception exception)
        {
            var message = WmiErrorHelper.GetFriendlyMessage(exception);
            lblStatus.Text = "Falha ao carregar perfis.";
            _logService.AddError(message);
            _logService.AddError(exception.Message);

            MessageBox.Show(
                message,
                "Erro ao carregar perfis",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        btnLoadProfiles.Enabled = !isLoading;
        txtComputerName.Enabled = !isLoading;
        UseWaitCursor = isLoading;

        if (isLoading)
        {
            lblStatus.Text = "Carregando perfis...";
        }
    }
}
