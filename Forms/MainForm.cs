using System.ComponentModel;
using RemovedorPerfisWindows.Controls;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;
using RemovedorPerfisWindows.Services;

namespace RemovedorPerfisWindows.Forms;

public partial class MainForm : Form
{
    private const int CollapsedStatusTop = 232;
    private const int ExpandedStatusTop = 292;
    private const int LegendGap = 10;
    private const int GridGap = 4;
    private const int LogsGap = 12;
    private const int LogsHeaderHeight = 30;
    private const int LogsHeaderToTextGap = 8;
    private const int LogsHeight = 125;
    private const int SideMargin = 28;

    private readonly LogService _logService = new();
    private readonly BindingList<UserProfileInfo> _profiles = [];
    private readonly List<UserProfileInfo> _allProfiles = [];
    private UserProfileQueryService _userProfileQueryService = null!;
    private ProfileSizeService _profileSizeService = null!;
    private UserProfileRemovalService _userProfileRemovalService = null!;
    private CancellationTokenSource? _sizeCalculationCancellation;
    private AppThemeMode _themeMode = AppThemeMode.Light;
    private ThemePalette _themePalette = ThemeHelper.GetPalette(AppThemeMode.Light);

    public MainForm()
    {
        InitializeComponent();
        Icon = SystemIcons.Shield;

        _userProfileQueryService = new UserProfileQueryService(_logService);
        _profileSizeService = new ProfileSizeService(_logService);
        _userProfileRemovalService = new UserProfileRemovalService(_logService);
        dgvProfiles.AutoGenerateColumns = false;
        dgvProfiles.DataSource = _profiles;
        ApplyAdvancedSettingsVisibility();
        ApplyTechnicalColumnsVisibility();
        ApplyTheme();

        _logService.EntryAdded += OnLogEntryAdded;
        _logService.AddInfo("Aplicativo iniciado. Informe o computador remoto e clique em Carregar perfis.");
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

    private void ChkShowAdvancedSettings_CheckedChanged(object? sender, EventArgs e)
    {
        ApplyAdvancedSettingsVisibility();
    }

    private void ChkShowTechnicalDetails_CheckedChanged(object? sender, EventArgs e)
    {
        ApplyTechnicalColumnsVisibility();
    }

    private void SetLoadingState(bool isLoading)
    {
        btnLoadProfiles.Enabled = !isLoading;
        txtComputerName.Enabled = !isLoading;
        chkUseAdminCredential.Enabled = !isLoading;
        chkCalculateProfileSize.Enabled = !isLoading;
        chkShowAdvancedSettings.Enabled = !isLoading;
        chkShowSystemProfiles.Enabled = !isLoading;
        chkShowTechnicalDetails.Enabled = !isLoading;
        btnCancelSizeCalculation.Enabled = false;
        btnRemoveSelected.Enabled = !isLoading && HasSelectedRemovableProfiles();
        btnCalculateSelectedSize.Enabled = !isLoading && HasSingleSelectedSizeCalculationProfile();
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
            ApplyProfileRowStyles();
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

            if (string.Equals(profile.OperationStatus, "Removido com sucesso", StringComparison.OrdinalIgnoreCase)
                || string.Equals(profile.OperationStatus, "Removido", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = _themePalette.RemovedRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }
            else if (string.Equals(profile.OperationStatus, "Ignorado", StringComparison.OrdinalIgnoreCase)
                || string.Equals(profile.OperationStatus, "Não confirmado", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = _themePalette.AttentionRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }
            else if (string.Equals(profile.OperationStatus, "Erro ao remover", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = _themePalette.BlockedRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }
            else if (profile.IsSelected && profile.CanRemove)
            {
                row.DefaultCellStyle.BackColor = _themePalette.SelectedForActionRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }
            else if (profile.Observation.Contains("Nome duplicado", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = _themePalette.AttentionRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }
            else if (profile.IsLoaded)
            {
                row.DefaultCellStyle.BackColor = _themePalette.BlockedRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }
            else if (!profile.CanRemove)
            {
                row.DefaultCellStyle.BackColor = profile.IsSystemOrServiceProfile || profile.BlockReason.Contains("protegido", StringComparison.OrdinalIgnoreCase)
                    ? _themePalette.ProtectedRowColor
                    : _themePalette.BlockedRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }
            else
            {
                row.DefaultCellStyle.BackColor = _themePalette.AvailableRowColor;
                row.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
            }

            row.DefaultCellStyle.ForeColor = _themePalette.GridForeColor;
            row.DefaultCellStyle.SelectionForeColor = _themePalette.GridSelectionForeColor;
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

        using (var confirmationForm = new RemovalConfirmationForm(computerName, selectedProfiles, _themeMode))
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
                result.Profile.OperationStatus = GetFriendlyRemovalStatus(result.Message);
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
        btnCalculateSelectedSize.Enabled = !isRemoving && HasSingleSelectedSizeCalculationProfile();
        btnCancelSizeCalculation.Enabled = false;
        txtComputerName.Enabled = !isRemoving;
        chkUseAdminCredential.Enabled = !isRemoving;
        chkCalculateProfileSize.Enabled = !isRemoving;
        chkShowAdvancedSettings.Enabled = !isRemoving;
        chkShowSystemProfiles.Enabled = !isRemoving;
        chkShowTechnicalDetails.Enabled = !isRemoving;
        dgvProfiles.Enabled = !isRemoving;
        UseWaitCursor = isRemoving;
    }

    private void UpdateRemoveButtonState()
    {
        btnRemoveSelected.Enabled = HasSelectedRemovableProfiles();
        btnCalculateSelectedSize.Enabled = HasSingleSelectedSizeCalculationProfile();
    }

    private bool HasSelectedRemovableProfiles()
    {
        return _profiles.Any(profile => profile.IsSelected && profile.CanRemove);
    }

    private bool HasSingleSelectedSizeCalculationProfile()
    {
        return GetSelectedSizeCalculationProfiles().Count == 1;
    }

    private List<UserProfileInfo> GetSelectedSizeCalculationProfiles()
    {
        return _profiles
            .Where(profile => profile.IsSelected && CanCalculateProfileSize(profile))
            .ToList();
    }

    private static bool CanCalculateProfileSize(UserProfileInfo profile)
    {
        return profile.CanRemove
            && !profile.IsSystemOrServiceProfile
            && UserProfileSafetyHelper.IsUsersProfilePath(profile.LocalPath);
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
        return $"{totalCount} perfis encontrados | {visibleCount} exibidos | {removableCount} disponíveis | {blockedCount} bloqueados{duplicateText}.";
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
            UpdateRemoveButtonState();
            _sizeCalculationCancellation.Dispose();
            _sizeCalculationCancellation = null;
        }
    }

    private async void BtnCalculateSelectedSize_Click(object? sender, EventArgs e)
    {
        var computerName = txtComputerName.Text.Trim();
        if (string.IsNullOrWhiteSpace(computerName))
        {
            _logService.AddWarning("Informe o nome do computador remoto antes de calcular o tamanho do perfil selecionado.");
            return;
        }

        var selectedProfiles = GetSelectedSizeCalculationProfiles();
        if (selectedProfiles.Count != 1)
        {
            lblStatus.Text = "selecione exatamente um perfil disponível para calcular o tamanho.";
            _logService.AddWarning("Cálculo individual não iniciado: selecione exatamente um perfil disponível.");
            UpdateRemoveButtonState();
            return;
        }

        AdminCredentialInfo? credential = null;

        if (chkUseAdminCredential.Checked)
        {
            using var credentialForm = new CredentialForm();
            if (credentialForm.ShowDialog(this) != DialogResult.OK || credentialForm.Credential is null)
            {
                _logService.AddWarning("Credencial administrativa cancelada. Cálculo individual cancelado.");
                lblStatus.Text = "cálculo cancelado.";
                return;
            }

            credential = credentialForm.Credential;
        }

        var profile = selectedProfiles[0];
        profile.SizeDisplay = "Calculando...";
        _logService.AddInfo($"Iniciando cálculo individual de tamanho para {profile}.");
        lblStatus.Text = "calculando tamanho do perfil selecionado...";
        dgvProfiles.Refresh();

        SetSizeCalculationState(isCalculating: true);

        _sizeCalculationCancellation?.Dispose();
        _sizeCalculationCancellation = new CancellationTokenSource();

        var progress = new Progress<ProfileSizeResult>(result =>
        {
            result.Profile.SizeDisplay = GetIndividualSizeDisplayText(result);
            dgvProfiles.Refresh();
        });

        try
        {
            await _profileSizeService.CalculateSizesAsync(
                computerName,
                selectedProfiles,
                credential,
                progress,
                _sizeCalculationCancellation.Token);

            lblStatus.Text = _sizeCalculationCancellation.IsCancellationRequested
                ? "cálculo cancelado."
                : "tamanho calculado para o perfil selecionado.";

            _logService.AddInfo($"Cálculo individual concluído para {profile}: {profile.SizeDisplay}.");
        }
        catch (OperationCanceledException)
        {
            profile.SizeDisplay = "Cancelado";
            _logService.AddWarning($"Cálculo individual cancelado para {profile}.");
            lblStatus.Text = "cálculo cancelado.";
        }
        finally
        {
            credential?.Clear();
            _sizeCalculationCancellation.Dispose();
            _sizeCalculationCancellation = null;
            SetSizeCalculationState(isCalculating: false);
            UpdateRemoveButtonState();
        }
    }

    private void SetSizeCalculationState(bool isCalculating)
    {
        btnLoadProfiles.Enabled = !isCalculating;
        btnRemoveSelected.Enabled = !isCalculating && HasSelectedRemovableProfiles();
        btnCalculateSelectedSize.Enabled = false;
        btnCancelSizeCalculation.Enabled = isCalculating;
        txtComputerName.Enabled = !isCalculating;
        chkUseAdminCredential.Enabled = !isCalculating;
        chkCalculateProfileSize.Enabled = !isCalculating;
        chkShowAdvancedSettings.Enabled = !isCalculating;
        chkShowSystemProfiles.Enabled = !isCalculating;
        chkShowTechnicalDetails.Enabled = !isCalculating;
        UseWaitCursor = isCalculating;
    }

    private static string GetIndividualSizeDisplayText(ProfileSizeResult result)
    {
        if (string.Equals(result.DisplayText, "Calculando...", StringComparison.OrdinalIgnoreCase))
        {
            return result.DisplayText;
        }

        return result.Status switch
        {
            ProfileSizeResultStatus.AccessDenied => "Requer permissão admin",
            ProfileSizeResultStatus.Error => "Não foi possível calcular",
            ProfileSizeResultStatus.Ignored => "Não aplicável",
            _ => result.DisplayText
        };
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

    private void BtnThemeToggle_Click(object? sender, EventArgs e)
    {
        _themeMode = _themeMode == AppThemeMode.Light ? AppThemeMode.Dark : AppThemeMode.Light;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        _themePalette = ThemeHelper.GetPalette(_themeMode);

        BackColor = _themePalette.FormBackColor;
        ForeColor = _themePalette.TextColor;

        ApplyThemeToControls(Controls);
        ApplyGridTheme();
        ApplyButtonTheme(btnLoadProfiles, primary: true);
        ApplyButtonTheme(btnRemoveSelected, critical: true);
        ApplyButtonTheme(btnCalculateSelectedSize);
        ApplyButtonTheme(btnCancelSizeCalculation);
        ApplyButtonTheme(btnClearLog);
        ApplyButtonTheme(btnCopyLog);
        ApplyThemeToggleButton();

        lblTitle.ForeColor = _themePalette.TitleColor;
        lblTitle.Font = new Font(lblTitle.Font, FontStyle.Bold);
        lblDescription.ForeColor = _themePalette.MutedTextColor;
        lblStepsHelp.ForeColor = _themePalette.MutedTextColor;
        lblLegend.ForeColor = _themePalette.MutedTextColor;
        lblAuthor.ForeColor = _themeMode == AppThemeMode.Dark ? _themePalette.AccentColor : _themePalette.TitleColor;
        lblAuthor.Font = new Font(lblAuthor.Font, FontStyle.Regular);
        lblStatusTitle.Font = new Font(lblStatusTitle.Font, FontStyle.Bold);
        lblLogs.Font = new Font(lblLogs.Font, FontStyle.Bold);
        txtLogs.BackColor = _themePalette.LogBackColor;
        txtLogs.ForeColor = _themePalette.LogForeColor;

        ApplyProfileRowStyles();
    }

    private void ApplyThemeToControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            switch (control)
            {
                case GroupBox groupBox:
                    groupBox.BackColor = _themePalette.PanelBackColor;
                    groupBox.ForeColor = _themePalette.TextColor;
                    groupBox.Font = new Font(groupBox.Font, FontStyle.Bold);
                    break;
                case Label label:
                    label.BackColor = Color.Transparent;
                    label.ForeColor = _themePalette.TextColor;
                    label.Font = new Font(label.Font, FontStyle.Regular);
                    break;
                case TextBox textBox:
                    textBox.BackColor = _themePalette.InputBackColor;
                    textBox.ForeColor = _themePalette.TextColor;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.Font = new Font(textBox.Font, FontStyle.Regular);
                    break;
                case CheckBox checkBox:
                    checkBox.BackColor = _themePalette.PanelBackColor;
                    checkBox.ForeColor = _themePalette.TextColor;
                    checkBox.Font = new Font(checkBox.Font, FontStyle.Regular);
                    break;
                case Button button:
                    button.Font = new Font(button.Font, FontStyle.Regular);
                    ApplyButtonTheme(button);
                    break;
            }

            if (control.HasChildren)
            {
                ApplyThemeToControls(control.Controls);
            }
        }
    }

    private void ApplyButtonTheme(Button button, bool primary = false, bool critical = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.EnabledChanged -= ThemedButton_EnabledChanged;
        button.EnabledChanged += ThemedButton_EnabledChanged;

        if (!button.Enabled)
        {
            button.BackColor = _themePalette.DisabledButtonBackColor;
            button.ForeColor = _themePalette.DisabledButtonForeColor;
            button.FlatAppearance.BorderColor = _themePalette.DisabledButtonBorderColor;
            button.FlatAppearance.MouseOverBackColor = _themePalette.DisabledButtonBackColor;
            button.FlatAppearance.MouseDownBackColor = _themePalette.DisabledButtonBackColor;
            ApplyThemedButtonColors(
                button,
                _themePalette.DisabledButtonBackColor,
                _themePalette.DisabledButtonForeColor,
                _themePalette.DisabledButtonBorderColor,
                _themePalette.DisabledButtonBackColor,
                _themePalette.DisabledButtonBackColor);
            button.UseVisualStyleBackColor = false;
            return;
        }

        if (primary)
        {
            button.BackColor = _themePalette.PrimaryButtonBackColor;
            button.ForeColor = _themePalette.PrimaryButtonForeColor;
            button.FlatAppearance.BorderColor = _themePalette.PrimaryButtonBorderColor;
            button.FlatAppearance.MouseOverBackColor = _themePalette.PrimaryButtonHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = _themePalette.PrimaryButtonPressedBackColor;
            ApplyThemedButtonColors(
                button,
                _themePalette.PrimaryButtonBackColor,
                _themePalette.PrimaryButtonForeColor,
                _themePalette.PrimaryButtonBorderColor,
                _themePalette.PrimaryButtonHoverBackColor,
                _themePalette.PrimaryButtonPressedBackColor);
        }
        else if (critical)
        {
            button.BackColor = _themePalette.CriticalButtonBackColor;
            button.ForeColor = _themePalette.CriticalButtonForeColor;
            button.FlatAppearance.BorderColor = _themePalette.CriticalButtonBorderColor;
            button.FlatAppearance.MouseOverBackColor = _themePalette.CriticalButtonHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = _themePalette.CriticalButtonPressedBackColor;
            ApplyThemedButtonColors(
                button,
                _themePalette.CriticalButtonBackColor,
                _themePalette.CriticalButtonForeColor,
                _themePalette.CriticalButtonBorderColor,
                _themePalette.CriticalButtonHoverBackColor,
                _themePalette.CriticalButtonPressedBackColor);
        }
        else
        {
            button.BackColor = _themePalette.SecondaryButtonBackColor;
            button.ForeColor = _themePalette.SecondaryButtonForeColor;
            button.FlatAppearance.BorderColor = _themePalette.SecondaryButtonBorderColor;
            button.FlatAppearance.MouseOverBackColor = _themePalette.SecondaryButtonHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = _themePalette.SecondaryButtonPressedBackColor;
            ApplyThemedButtonColors(
                button,
                _themePalette.SecondaryButtonBackColor,
                _themePalette.SecondaryButtonForeColor,
                _themePalette.SecondaryButtonBorderColor,
                _themePalette.SecondaryButtonHoverBackColor,
                _themePalette.SecondaryButtonPressedBackColor);
        }

        button.UseVisualStyleBackColor = false;
    }

    private void ApplyThemedButtonColors(
        Button button,
        Color backColor,
        Color foreColor,
        Color borderColor,
        Color hoverBackColor,
        Color pressedBackColor)
    {
        if (button is not ThemedButton themedButton)
        {
            return;
        }

        themedButton.BackColor = backColor;
        themedButton.ForeColor = foreColor;
        themedButton.ButtonBorderColor = borderColor;
        themedButton.HoverBackColor = hoverBackColor;
        themedButton.PressedBackColor = pressedBackColor;
        themedButton.DisabledBackColor = _themePalette.DisabledButtonBackColor;
        themedButton.DisabledForeColor = _themePalette.DisabledButtonForeColor;
        themedButton.DisabledBorderColor = _themePalette.DisabledButtonBorderColor;
        themedButton.Invalidate();
    }

    private void ApplyThemeToggleButton()
    {
        btnThemeToggle.Text = _themeMode == AppThemeMode.Light ? "☾" : "☀";
        btnThemeToggle.BackColor = _themeMode == AppThemeMode.Light
            ? _themePalette.SecondaryButtonBackColor
            : _themePalette.PrimaryButtonBackColor;
        btnThemeToggle.ForeColor = _themeMode == AppThemeMode.Light
            ? _themePalette.TitleColor
            : _themePalette.PrimaryButtonForeColor;
        btnThemeToggle.FlatAppearance.BorderColor = _themeMode == AppThemeMode.Light
            ? _themePalette.SecondaryButtonBorderColor
            : _themePalette.PrimaryButtonBorderColor;
        btnThemeToggle.FlatAppearance.MouseOverBackColor = _themeMode == AppThemeMode.Light
            ? _themePalette.SecondaryButtonHoverBackColor
            : _themePalette.PrimaryButtonHoverBackColor;
        btnThemeToggle.FlatAppearance.MouseDownBackColor = _themeMode == AppThemeMode.Light
            ? _themePalette.SecondaryButtonPressedBackColor
            : _themePalette.PrimaryButtonPressedBackColor;
        ApplyThemedButtonColors(
            btnThemeToggle,
            btnThemeToggle.BackColor,
            btnThemeToggle.ForeColor,
            btnThemeToggle.FlatAppearance.BorderColor,
            btnThemeToggle.FlatAppearance.MouseOverBackColor,
            btnThemeToggle.FlatAppearance.MouseDownBackColor);
        btnThemeToggle.UseVisualStyleBackColor = false;
    }

    private void ThemedButton_EnabledChanged(object? sender, EventArgs e)
    {
        if (sender == btnLoadProfiles)
        {
            ApplyButtonTheme(btnLoadProfiles, primary: true);
            return;
        }

        if (sender == btnRemoveSelected)
        {
            ApplyButtonTheme(btnRemoveSelected, critical: true);
            return;
        }

        if (sender is Button button)
        {
            ApplyButtonTheme(button);
        }
    }

    private void ApplyGridTheme()
    {
        dgvProfiles.EnableHeadersVisualStyles = false;
        dgvProfiles.BackgroundColor = _themePalette.GridBackColor;
        dgvProfiles.GridColor = _themePalette.GridLineColor;
        dgvProfiles.BorderStyle = BorderStyle.FixedSingle;
        dgvProfiles.ColumnHeadersDefaultCellStyle.BackColor = _themePalette.GridHeaderBackColor;
        dgvProfiles.ColumnHeadersDefaultCellStyle.ForeColor = _themePalette.GridHeaderForeColor;
        dgvProfiles.ColumnHeadersDefaultCellStyle.Font = new Font(dgvProfiles.Font, FontStyle.Bold);
        dgvProfiles.ColumnHeadersDefaultCellStyle.SelectionBackColor = _themePalette.GridHeaderBackColor;
        dgvProfiles.ColumnHeadersDefaultCellStyle.SelectionForeColor = _themePalette.GridHeaderForeColor;
        dgvProfiles.DefaultCellStyle.BackColor = _themePalette.GridBackColor;
        dgvProfiles.DefaultCellStyle.ForeColor = _themePalette.GridForeColor;
        dgvProfiles.DefaultCellStyle.SelectionBackColor = _themePalette.GridSelectionBackColor;
        dgvProfiles.DefaultCellStyle.SelectionForeColor = _themePalette.GridSelectionForeColor;
        dgvProfiles.AlternatingRowsDefaultCellStyle.BackColor = _themePalette.GridBackColor;
        dgvProfiles.AlternatingRowsDefaultCellStyle.ForeColor = _themePalette.GridForeColor;
    }

    private void ApplyAdvancedSettingsVisibility()
    {
        grpAdvancedSettings.Visible = chkShowAdvancedSettings.Checked;

        var statusTop = chkShowAdvancedSettings.Checked ? ExpandedStatusTop : CollapsedStatusTop;
        grpStatus.Top = statusTop;
        lblLegend.Top = grpStatus.Bottom + LegendGap;
        dgvProfiles.Top = lblLegend.Bottom + GridGap;
        ResizeMainSections();
    }

    private void ApplyTechnicalColumnsVisibility()
    {
        var showTechnicalDetails = chkShowTechnicalDetails.Checked;
        colSid.Visible = showTechnicalDetails;
        colLocalPath.Visible = showTechnicalDetails;
        colOperationStatus.Visible = showTechnicalDetails;
        colObservation.Visible = showTechnicalDetails;
    }

    private void ResizeMainSections()
    {
        if (txtLogs is null || dgvProfiles is null)
        {
            return;
        }

        var logsTop = ClientSize.Height - SideMargin - LogsHeight;
        var logsHeaderTop = logsTop - LogsHeaderToTextGap - LogsHeaderHeight;
        var gridBottom = logsHeaderTop - LogsGap;

        dgvProfiles.Height = Math.Max(180, gridBottom - dgvProfiles.Top);
        lblLogs.Top = logsHeaderTop + 7;
        btnClearLog.Top = logsHeaderTop;
        btnCopyLog.Top = logsHeaderTop;
        txtLogs.Top = logsTop;
        txtLogs.Height = LogsHeight;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ResizeMainSections();
    }

    private static string GetFriendlyRemovalStatus(string message)
    {
        return string.Equals(message, "Removido", StringComparison.OrdinalIgnoreCase)
            ? "Removido com sucesso"
            : message;
    }
}
