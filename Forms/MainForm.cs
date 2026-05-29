using System.ComponentModel;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;
using RemovedorPerfisWindows.Services;

namespace RemovedorPerfisWindows.Forms;

public partial class MainForm : Form
{
    private static readonly Color BlockedRowColor = Color.FromArgb(235, 235, 235);
    private static readonly Color LoadedRowColor = Color.FromArgb(255, 230, 230);

    private readonly LogService _logService = new();
    private readonly BindingList<UserProfileInfo> _profiles = [];
    private UserProfileQueryService _userProfileQueryService = null!;
    private ProfileSizeService _profileSizeService = null!;
    private CancellationTokenSource? _sizeCalculationCancellation;

    public MainForm()
    {
        InitializeComponent();

        _userProfileQueryService = new UserProfileQueryService(_logService);
        _profileSizeService = new ProfileSizeService(_logService);
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

        AdminCredentialInfo? credential = null;

        if (chkUseAdminCredential.Checked)
        {
            using var credentialForm = new CredentialForm();
            if (credentialForm.ShowDialog(this) != DialogResult.OK || credentialForm.Credential is null)
            {
                _logService.AddWarning("Credencial administrativa cancelada. Operação de carregamento de perfis cancelada.");
                lblStatus.Text = "Operação cancelada pelo usuário.";
                return;
            }

            credential = credentialForm.Credential;
        }

        SetLoadingState(true);
        _profiles.Clear();
        _logService.AddInfo($"Solicitação de carregamento de perfis para {computerName}.");

        try
        {
            var profiles = await _userProfileQueryService.GetProfilesAsync(computerName, credential);

            foreach (var profile in profiles)
            {
                profile.SizeDisplay = "Não calculado";
                _profiles.Add(profile);
            }

            var removableCount = profiles.Count(profile => profile.CanRemove);
            var blockedCount = profiles.Count - removableCount;
            var duplicateCount = profiles.Count(profile => profile.Observation.Contains("Nome duplicado", StringComparison.OrdinalIgnoreCase));
            lblStatus.Text = duplicateCount > 0
                ? $"{profiles.Count} perfil(is): {removableCount} disponível(is), {blockedCount} bloqueado(s), {duplicateCount} com nome duplicado."
                : $"{profiles.Count} perfil(is) encontrado(s): {removableCount} disponível(is), {blockedCount} bloqueado(s).";
            _logService.AddInfo("Consulta concluída com sucesso.");

            if (chkCalculateProfileSize.Checked)
            {
                await CalculateProfileSizesAsync(computerName, profiles, credential);
            }
            else
            {
                _logService.AddInfo("Cálculo de tamanho dos perfis desativado.");
            }
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
            credential?.Clear();
            SetLoadingState(false);
        }
    }

    private void BtnCancelSizeCalculation_Click(object? sender, EventArgs e)
    {
        if (_sizeCalculationCancellation is null)
        {
            return;
        }

        _logService.AddWarning("Cancelamento do cálculo de tamanho solicitado pelo usuário.");
        lblStatus.Text = "Cancelando cálculo de tamanho...";
        btnCancelSizeCalculation.Enabled = false;
        _sizeCalculationCancellation.Cancel();
    }

    private void SetLoadingState(bool isLoading)
    {
        btnLoadProfiles.Enabled = !isLoading;
        txtComputerName.Enabled = !isLoading;
        chkUseAdminCredential.Enabled = !isLoading;
        chkCalculateProfileSize.Enabled = !isLoading;
        btnCancelSizeCalculation.Enabled = false;
        UseWaitCursor = isLoading;

        if (isLoading)
        {
            lblStatus.Text = "Carregando perfis...";
        }
    }

    private void DgvProfiles_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colSelection.Index)
        {
            return;
        }

        var profile = GetProfileFromRow(e.RowIndex);
        if (profile?.CanRemove != false)
        {
            return;
        }

        e.Cancel = true;
        profile.IsSelected = false;
        dgvProfiles.InvalidateRow(e.RowIndex);
        _logService.AddWarning($"Seleção bloqueada para {profile}: {profile.BlockReason}.");
    }

    private void DgvProfiles_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colSelection.Index)
        {
            return;
        }

        var profile = GetProfileFromRow(e.RowIndex);
        if (profile?.CanRemove == false)
        {
            profile.IsSelected = false;
            dgvProfiles.InvalidateRow(e.RowIndex);
            _logService.AddWarning($"Tentativa de seleção bloqueada para {profile}: {profile.BlockReason}.");
        }
    }

    private void DgvProfiles_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dgvProfiles.IsCurrentCellDirty)
        {
            dgvProfiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void DgvProfiles_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colSelection.Index)
        {
            return;
        }

        var profile = GetProfileFromRow(e.RowIndex);
        if (profile is null || profile.CanRemove)
        {
            return;
        }

        profile.IsSelected = false;
        dgvProfiles.Rows[e.RowIndex].Cells[colSelection.Index].Value = false;
        dgvProfiles.InvalidateRow(e.RowIndex);
        _logService.AddWarning($"Seleção desfeita para {profile}: {profile.BlockReason}.");
    }

    private void DgvProfiles_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        ApplyProfileRowStyles();
    }

    private void ApplyProfileRowStyles()
    {
        foreach (DataGridViewRow row in dgvProfiles.Rows)
        {
            if (row.DataBoundItem is not UserProfileInfo profile)
            {
                continue;
            }

            row.Cells[colSelection.Index].ReadOnly = !profile.CanRemove;
            row.Cells[colSelection.Index].ToolTipText = profile.CanRemove ? string.Empty : profile.BlockReason;

            if (profile.IsLoaded)
            {
                row.DefaultCellStyle.BackColor = LoadedRowColor;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 160, 160);
            }
            else if (!profile.CanRemove)
            {
                row.DefaultCellStyle.BackColor = BlockedRowColor;
                row.DefaultCellStyle.SelectionBackColor = Color.Gray;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            }
        }
    }

    private UserProfileInfo? GetProfileFromRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= dgvProfiles.Rows.Count)
        {
            return null;
        }

        return dgvProfiles.Rows[rowIndex].DataBoundItem as UserProfileInfo;
    }

    private async Task CalculateProfileSizesAsync(
        string computerName,
        IReadOnlyList<UserProfileInfo> profiles,
        AdminCredentialInfo? credential)
    {
        lblStatus.Text = "Calculando tamanho dos perfis...";

        _sizeCalculationCancellation?.Dispose();
        _sizeCalculationCancellation = new CancellationTokenSource();
        btnCancelSizeCalculation.Enabled = true;

        var progress = new Progress<ProfileSizeResult>(result =>
        {
            result.Profile.SizeDisplay = result.DisplayText;
            dgvProfiles.Refresh();
        });

        try
        {
            await _profileSizeService.CalculateSizesAsync(
                computerName,
                profiles,
                credential,
                progress,
                _sizeCalculationCancellation.Token);

            lblStatus.Text = _sizeCalculationCancellation.IsCancellationRequested
                ? "Cálculo de tamanho cancelado."
                : "Cálculo de tamanho concluído.";
        }
        catch (OperationCanceledException)
        {
            foreach (var profile in profiles.Where(profile => profile.SizeDisplay is "Não calculado" or "Calculando..."))
            {
                profile.SizeDisplay = "Cancelado";
            }

            _logService.AddWarning("Cálculo de tamanho cancelado pelo usuário.");
            lblStatus.Text = "Cálculo de tamanho cancelado.";
        }
        finally
        {
            btnCancelSizeCalculation.Enabled = false;
            _sizeCalculationCancellation.Dispose();
            _sizeCalculationCancellation = null;
        }
    }
}
