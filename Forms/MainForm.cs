using System.ComponentModel;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;
using RemovedorPerfisWindows.Services;

namespace RemovedorPerfisWindows.Forms;

public partial class MainForm : Form
{
    private static readonly Color BlockedRowColor = Color.FromArgb(235, 235, 235);
    private static readonly Color LoadedRowColor = Color.FromArgb(255, 230, 230);
    private static readonly Color RemovedRowColor = Color.FromArgb(220, 245, 220);
    private static readonly Color SkippedRowColor = Color.FromArgb(255, 242, 204);
    private static readonly Color ErrorRowColor = Color.FromArgb(255, 220, 220);

    private readonly LogService _logService = new();
    private readonly BindingList<UserProfileInfo> _profiles = [];
    private readonly List<UserProfileInfo> _allProfiles = [];
    private UserProfileQueryService _userProfileQueryService = null!;
    private ProfileSizeService _profileSizeService = null!;
    private UserProfileRemovalService _userProfileRemovalService = null!;
    private CancellationTokenSource? _sizeCalculationCancellation;

    public MainForm()
    {
        InitializeComponent();

        _userProfileQueryService = new UserProfileQueryService(_logService);
        _profileSizeService = new ProfileSizeService(_logService);
        _userProfileRemovalService = new UserProfileRemovalService(_logService);
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
                lblStatus.Text = "operação cancelada pelo usuário.";
                return;
            }

            credential = credentialForm.Credential;
        }

        SetLoadingState(true);
        _profiles.Clear();
        _allProfiles.Clear();
        _logService.AddInfo($"Solicitação de carregamento de perfis para {computerName}.");

        try
        {
            var profiles = await _userProfileQueryService.GetProfilesAsync(computerName, credential);

            foreach (var profile in profiles)
            {
                profile.SizeDisplay = "Não calculado";
                profile.OperationStatus = string.Empty;
                _allProfiles.Add(profile);
            }

            ApplyProfileVisibilityFilter();

            UpdateRemoveButtonState();

            var visibleProfiles = _profiles.ToList();
            var removableCount = visibleProfiles.Count(profile => profile.CanRemove);
            var blockedCount = visibleProfiles.Count - removableCount;
            var duplicateCount = profiles.Count(profile => profile.Observation.Contains("Nome duplicado", StringComparison.OrdinalIgnoreCase));
            lblStatus.Text = BuildProfileSummaryText(profiles.Count, visibleProfiles.Count, removableCount, blockedCount, duplicateCount);
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
            lblStatus.Text = "falha ao carregar perfis.";
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
        lblStatus.Text = "cancelando cálculo de tamanho...";
        btnCancelSizeCalculation.Enabled = false;
        _sizeCalculationCancellation.Cancel();
    }

    private void ChkShowSystemProfiles_CheckedChanged(object? sender, EventArgs e)
    {
        ApplyProfileVisibilityFilter();

        if (chkShowSystemProfiles.Checked)
        {
            _logService.AddWarning("Perfis de sistema/serviço estão visíveis apenas para análise e continuam bloqueados.");
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        btnLoadProfiles.Enabled = !isLoading;
        txtComputerName.Enabled = !isLoading;
        chkUseAdminCredential.Enabled = !isLoading;
        chkCalculateProfileSize.Enabled = !isLoading;
        chkShowSystemProfiles.Enabled = !isLoading;
        btnCancelSizeCalculation.Enabled = false;
        btnRemoveSelected.Enabled = !isLoading && HasSelectedRemovableProfiles();
        UseWaitCursor = isLoading;

        if (isLoading)
        {
            lblStatus.Text = "carregando perfis...";
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
            UpdateRemoveButtonState();
            return;
        }

        profile.IsSelected = false;
        dgvProfiles.Rows[e.RowIndex].Cells[colSelection.Index].Value = false;
        dgvProfiles.InvalidateRow(e.RowIndex);
        _logService.AddWarning($"Seleção desfeita para {profile}: {profile.BlockReason}.");
        UpdateRemoveButtonState();
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

            if (string.Equals(profile.OperationStatus, "Removido", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = RemovedRowColor;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(140, 210, 140);
            }
            else if (string.Equals(profile.OperationStatus, "Ignorado", StringComparison.OrdinalIgnoreCase)
                || string.Equals(profile.OperationStatus, "Não confirmado", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = SkippedRowColor;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 190, 110);
            }
            else if (string.Equals(profile.OperationStatus, "Erro ao remover", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = ErrorRowColor;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 150, 150);
            }
            else if (profile.IsLoaded)
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

    private async void BtnRemoveSelected_Click(object? sender, EventArgs e)
    {
        var computerName = txtComputerName.Text.Trim();
        if (string.IsNullOrWhiteSpace(computerName))
        {
            _logService.AddWarning("Informe o nome do computador remoto antes de remover perfis.");
            return;
        }

        var selectedProfiles = _profiles
            .Where(profile => profile.IsSelected && profile.CanRemove)
            .ToList();

        if (selectedProfiles.Count == 0)
        {
            _logService.AddWarning("Nenhum perfil removível selecionado para remoção.");
            UpdateRemoveButtonState();
            return;
        }

        using (var confirmationForm = new RemovalConfirmationForm(computerName, selectedProfiles))
        {
            if (confirmationForm.ShowDialog(this) != DialogResult.OK)
            {
                _logService.AddWarning("Remoção cancelada na confirmação forte.");
                return;
            }
        }

        AdminCredentialInfo? credential = null;

        if (chkUseAdminCredential.Checked)
        {
            using var credentialForm = new CredentialForm();
            if (credentialForm.ShowDialog(this) != DialogResult.OK || credentialForm.Credential is null)
            {
                _logService.AddWarning("Credencial administrativa cancelada. Remoção cancelada.");
                lblStatus.Text = "remoção cancelada pelo usuário.";
                return;
            }

            credential = credentialForm.Credential;
        }

        SetRemovalState(true);
        lblStatus.Text = "removendo perfis selecionados...";

        foreach (var profile in selectedProfiles)
        {
            profile.OperationStatus = "Aguardando remoção";
        }

        try
        {
            var progress = new Progress<ProfileRemovalResult>(result =>
            {
                result.Profile.OperationStatus = result.Message;
                result.Profile.IsSelected = false;
                ApplyProfileRowStyles();
                dgvProfiles.Refresh();
            });

            var results = await _userProfileRemovalService.RemoveProfilesAsync(
                computerName,
                selectedProfiles,
                credential,
                progress);

            var removed = results.Count(result => result.Success);
            var skipped = results.Count(result => result.Skipped);
            var notConfirmed = results.Count(result => !result.Success && !result.Skipped && result.Message == "Não confirmado");
            var errors = results.Count(result => !result.Success && !result.Skipped && result.Message == "Erro ao remover");

            lblStatus.Text = $"remoção concluída: {removed} removido(s), {skipped} ignorado(s), {errors} erro(s), {notConfirmed} não confirmado(s).";
        }
        catch (Exception exception)
        {
            var message = WmiErrorHelper.GetFriendlyMessage(exception);
            lblStatus.Text = "falha durante remoção.";
            _logService.AddError(message);

            MessageBox.Show(
                message,
                "Erro ao remover perfis",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            credential?.Clear();
            SetRemovalState(false);
            UpdateRemoveButtonState();
            ApplyProfileRowStyles();
        }
    }

    private void SetRemovalState(bool isRemoving)
    {
        btnLoadProfiles.Enabled = !isRemoving;
        btnRemoveSelected.Enabled = !isRemoving && HasSelectedRemovableProfiles();
        btnCancelSizeCalculation.Enabled = false;
        txtComputerName.Enabled = !isRemoving;
        chkUseAdminCredential.Enabled = !isRemoving;
        chkCalculateProfileSize.Enabled = !isRemoving;
        chkShowSystemProfiles.Enabled = !isRemoving;
        dgvProfiles.Enabled = !isRemoving;
        UseWaitCursor = isRemoving;
    }

    private void UpdateRemoveButtonState()
    {
        btnRemoveSelected.Enabled = HasSelectedRemovableProfiles();
    }

    private bool HasSelectedRemovableProfiles()
    {
        return _profiles.Any(profile => profile.IsSelected && profile.CanRemove);
    }

    private void ApplyProfileVisibilityFilter()
    {
        var showSystemProfiles = chkShowSystemProfiles.Checked;
        var visibleProfiles = _allProfiles
            .Where(profile => showSystemProfiles || !profile.IsHiddenByDefault)
            .ToList();

        var hiddenSystemProfiles = _allProfiles.Count(profile => profile.IsHiddenByDefault && !showSystemProfiles);
        var visibleRemovable = visibleProfiles.Count(profile => profile.CanRemove);
        var visibleBlocked = visibleProfiles.Count - visibleRemovable;
        var duplicateCount = _allProfiles.Count(profile => profile.Observation.Contains("Nome duplicado", StringComparison.OrdinalIgnoreCase));

        _profiles.Clear();

        foreach (var profile in visibleProfiles)
        {
            _profiles.Add(profile);
        }

        _logService.AddInfo($"{_allProfiles.Count} perfil(is) total(is) retornado(s) pelo WMI.");
        _logService.AddInfo($"{visibleProfiles.Count} perfil(is) exibido(s) na grade.");
        _logService.AddInfo($"{hiddenSystemProfiles} perfil(is) ocultado(s) por serem de sistema/serviço.");
        _logService.AddInfo($"{visibleBlocked} perfil(is) bloqueado(s) exibido(s).");
        _logService.AddInfo($"{visibleRemovable} perfil(is) disponível(is) para análise exibido(s).");

        if (_allProfiles.Count > 0)
        {
            lblStatus.Text = BuildProfileSummaryText(_allProfiles.Count, visibleProfiles.Count, visibleRemovable, visibleBlocked, duplicateCount);
        }

        UpdateRemoveButtonState();
        ApplyProfileRowStyles();
    }

    private static string BuildProfileSummaryText(
        int totalCount,
        int visibleCount,
        int removableCount,
        int blockedCount,
        int duplicateCount)
    {
        var duplicateText = duplicateCount > 0 ? $", {duplicateCount} com nome duplicado" : string.Empty;
        return $"{visibleCount}/{totalCount} perfis exibidos: {removableCount} disponíveis, {blockedCount} bloqueados{duplicateText}.";
    }

    private async Task CalculateProfileSizesAsync(
        string computerName,
        IReadOnlyList<UserProfileInfo> profiles,
        AdminCredentialInfo? credential)
    {
        lblStatus.Text = "calculando tamanho dos perfis...";

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
                ? "cálculo de tamanho cancelado."
                : "cálculo de tamanho concluído.";
        }
        catch (OperationCanceledException)
        {
            foreach (var profile in profiles.Where(profile => profile.SizeDisplay is "Não calculado" or "Calculando..."))
            {
                profile.SizeDisplay = "Cancelado";
            }

            _logService.AddWarning("Cálculo de tamanho cancelado pelo usuário.");
            lblStatus.Text = "cálculo de tamanho cancelado.";
        }
        finally
        {
            btnCancelSizeCalculation.Enabled = false;
            _sizeCalculationCancellation.Dispose();
            _sizeCalculationCancellation = null;
        }
    }

    private void BtnClearLog_Click(object? sender, EventArgs e)
    {
        txtLogs.Clear();
        _logService.AddInfo("Log da interface limpo.");
    }

    private void BtnCopyLog_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLogs.Text))
        {
            _logService.AddWarning("Não há conteúdo de log para copiar.");
            return;
        }

        Clipboard.SetText(txtLogs.Text);
        _logService.AddInfo("Log copiado para a área de transferência.");
    }
}
