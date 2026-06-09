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
    private const int TopGroupsTop = 120;
    private const int TopGroupsHeight = 104;
    private const int TopGroupsGap = 16;
    private const int GroupPadding = 16;
    private const int ButtonGap = 12;
    private const int ActionButtonHeight = 28;
    private const int ActionButtonMaxWidth = 150;
    private const int CalculateButtonMaxWidth = 312;

    private readonly LogService _logService = new();
    private readonly BindingList<UserProfileInfo> _profiles = [];
    private readonly List<UserProfileInfo> _allProfiles = [];
    private UserProfileQueryService _userProfileQueryService = null!;
    private ProfileSizeService _profileSizeService = null!;
    private UserProfileRemovalService _userProfileRemovalService = null!;
    private CancellationTokenSource? _sizeCalculationCancellation;
    private AppThemeMode _themeMode = AppThemeMode.Light;
    private ThemePalette _themePalette = ThemeHelper.GetPalette(AppThemeMode.Light);
    private DataGridViewColumn? _currentSortColumn;
    private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;
    private bool _updatingSelectAllRemovable;

    public MainForm()
    {
        InitializeComponent();
        Icon = LoadWindowIcon();

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

    private static Icon LoadWindowIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "assets", "app-icon.ico");
        if (!File.Exists(iconPath))
        {
            return SystemIcons.Shield;
        }

        try
        {
            return new Icon(iconPath);
        }
        catch
        {
            return SystemIcons.Shield;
        }
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
                profile.SizeBytes = null;
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
            UpdateRemoveButtonState();
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
        btnCalculateSelectedSize.Enabled = !isLoading && HasSelectedSizeCalculationProfiles();
        chkSelectAllRemovable.Enabled = !isLoading && HasVisibleRemovableProfiles();
        SetBusyCursor(isLoading);

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

    private void DgvProfiles_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.ColumnIndex >= dgvProfiles.Columns.Count)
        {
            return;
        }

        var column = dgvProfiles.Columns[e.ColumnIndex];
        if (dgvProfiles.IsCurrentCellDirty)
        {
            dgvProfiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        dgvProfiles.EndEdit();

        var direction = _currentSortColumn == column && _currentSortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        _logService.AddInfo($"Ordenação solicitada pela coluna {column.HeaderText}.");
        SortVisibleProfiles(column, direction);
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
            row.Cells[colSid.Index].ToolTipText = profile.Sid;
            row.Cells[colLocalPath.Index].ToolTipText = profile.LocalPath;
            row.Cells[colStatus.Index].ToolTipText = profile.Status;
            row.Cells[colObservation.Index].ToolTipText = profile.Observation;

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
        btnCalculateSelectedSize.Enabled = !isRemoving && HasSelectedSizeCalculationProfiles();
        btnCancelSizeCalculation.Enabled = false;
        chkSelectAllRemovable.Enabled = !isRemoving && HasVisibleRemovableProfiles();
        txtComputerName.Enabled = !isRemoving;
        chkUseAdminCredential.Enabled = !isRemoving;
        chkCalculateProfileSize.Enabled = !isRemoving;
        chkShowAdvancedSettings.Enabled = !isRemoving;
        chkShowSystemProfiles.Enabled = !isRemoving;
        chkShowTechnicalDetails.Enabled = !isRemoving;
        dgvProfiles.Enabled = !isRemoving;
        SetBusyCursor(isRemoving);
    }

    private void UpdateRemoveButtonState()
    {
        btnRemoveSelected.Enabled = HasSelectedRemovableProfiles();
        btnCalculateSelectedSize.Enabled = HasSelectedSizeCalculationProfiles();
        UpdateSelectAllRemovableState();
    }

    private bool HasSelectedRemovableProfiles()
    {
        return _profiles.Any(profile => profile.IsSelected && profile.CanRemove);
    }

    private bool HasSelectedSizeCalculationProfiles()
    {
        return GetSelectedSizeCalculationProfiles().Count > 0;
    }

    private bool HasVisibleRemovableProfiles()
    {
        return _profiles.Any(profile => profile.CanRemove);
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

    private void ChkSelectAllRemovable_CheckedChanged(object? sender, EventArgs e)
    {
        if (_updatingSelectAllRemovable)
        {
            return;
        }

        var removableProfiles = _profiles
            .Where(profile => profile.CanRemove)
            .ToList();

        if (removableProfiles.Count == 0)
        {
            UpdateSelectAllRemovableState();
            return;
        }

        if (chkSelectAllRemovable.Checked)
        {
            foreach (var profile in removableProfiles)
            {
                profile.IsSelected = true;
            }

            _logService.AddInfo($"Selecionados {removableProfiles.Count} perfil(is) removível(is).");
        }
        else
        {
            foreach (var profile in removableProfiles)
            {
                profile.IsSelected = false;
            }

            _logService.AddInfo("Seleção de perfis removíveis limpa.");
        }

        dgvProfiles.Refresh();
        ApplyProfileRowStyles();
        UpdateRemoveButtonState();
    }

    private void UpdateSelectAllRemovableState()
    {
        if (chkSelectAllRemovable is null)
        {
            return;
        }

        var removableProfiles = _profiles
            .Where(profile => profile.CanRemove)
            .ToList();

        _updatingSelectAllRemovable = true;
        chkSelectAllRemovable.Enabled = removableProfiles.Count > 0
            && btnLoadProfiles.Enabled
            && dgvProfiles.Enabled
            && _sizeCalculationCancellation is null;
        chkSelectAllRemovable.Checked = removableProfiles.Count > 0
            && removableProfiles.All(profile => profile.IsSelected);
        _updatingSelectAllRemovable = false;
    }

    private void ApplyProfileVisibilityFilter()
    {
        var showSystemProfiles = chkShowSystemProfiles.Checked;
        var visibleProfiles = _allProfiles
            .Where(profile => showSystemProfiles || !profile.IsHiddenByDefault)
            .ToList();

        if (_currentSortColumn is not null)
        {
            visibleProfiles = SortProfiles(visibleProfiles, _currentSortColumn, _currentSortDirection).ToList();
        }

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
        ApplyDefaultColumnWidths();
        ApplyProfileRowStyles();
        ApplySortGlyph();
    }

    private void SortVisibleProfiles(DataGridViewColumn column, ListSortDirection direction)
    {
        var selectedProfileKeys = _profiles
            .Where(profile => profile.IsSelected)
            .Select(GetProfileKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sortedProfiles = SortProfiles(_profiles.ToList(), column, direction).ToList();

        _currentSortColumn = column;
        _currentSortDirection = direction;

        _profiles.Clear();
        foreach (var profile in sortedProfiles)
        {
            profile.IsSelected = profile.CanRemove && selectedProfileKeys.Contains(GetProfileKey(profile));
            _profiles.Add(profile);
        }

        ApplyProfileRowStyles();
        ApplySortGlyph();
        UpdateRemoveButtonState();
    }

    private IEnumerable<UserProfileInfo> SortProfiles(
        IEnumerable<UserProfileInfo> profiles,
        DataGridViewColumn column,
        ListSortDirection direction)
    {
        var comparer = Comparer<UserProfileInfo>.Create((left, right) =>
        {
            var result = CompareProfiles(left, right, column);
            return direction == ListSortDirection.Descending ? -result : result;
        });

        return profiles.OrderBy(profile => profile, comparer);
    }

    private int CompareProfiles(UserProfileInfo left, UserProfileInfo right, DataGridViewColumn column)
    {
        var result = column.Name switch
        {
            nameof(colSelection) => CompareBooleans(left.IsSelected, right.IsSelected),
            nameof(colUserName) => CompareText(left.UserName, right.UserName),
            nameof(colLastUseTime) => CompareNullableDates(left.LastUseTime, right.LastUseTime),
            nameof(colIsLoaded) => CompareBooleans(left.IsLoaded, right.IsLoaded),
            nameof(colSize) => CompareNullableLongs(left.SizeBytes, right.SizeBytes),
            nameof(colStatus) => CompareText(left.Status, right.Status),
            nameof(colSid) => CompareText(left.Sid, right.Sid),
            nameof(colLocalPath) => CompareText(left.LocalPath, right.LocalPath),
            nameof(colOperationStatus) => CompareText(left.OperationStatus, right.OperationStatus),
            nameof(colObservation) => CompareText(left.Observation, right.Observation),
            _ => 0
        };

        return result != 0 ? result : CompareText(left.UserName, right.UserName);
    }

    private static int CompareBooleans(bool left, bool right)
    {
        return left.CompareTo(right);
    }

    private static int CompareText(string left, string right)
    {
        return string.Compare(left, right, StringComparison.CurrentCultureIgnoreCase);
    }

    private static int CompareNullableDates(DateTime? left, DateTime? right)
    {
        if (left.HasValue && right.HasValue)
        {
            return left.Value.CompareTo(right.Value);
        }

        if (left.HasValue)
        {
            return -1;
        }

        return right.HasValue ? 1 : 0;
    }

    private static int CompareNullableLongs(long? left, long? right)
    {
        if (left.HasValue && right.HasValue)
        {
            return left.Value.CompareTo(right.Value);
        }

        if (left.HasValue)
        {
            return -1;
        }

        return right.HasValue ? 1 : 0;
    }

    private static string GetProfileKey(UserProfileInfo profile)
    {
        return string.IsNullOrWhiteSpace(profile.Sid) ? profile.LocalPath : profile.Sid;
    }

    private void ApplySortGlyph()
    {
        foreach (DataGridViewColumn column in dgvProfiles.Columns)
        {
            column.HeaderCell.SortGlyphDirection = SortOrder.None;
        }

        if (_currentSortColumn is null)
        {
            return;
        }

        _currentSortColumn.HeaderCell.SortGlyphDirection = _currentSortDirection == ListSortDirection.Ascending
            ? SortOrder.Ascending
            : SortOrder.Descending;
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
            result.Profile.SizeBytes = result.Bytes;
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
                profile.SizeBytes = null;
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
            _logService.AddWarning("Informe o nome do computador remoto antes de calcular o tamanho dos perfis selecionados.");
            return;
        }

        var selectedProfiles = GetSelectedSizeCalculationProfiles();
        if (selectedProfiles.Count == 0)
        {
            lblStatus.Text = "selecione ao menos um perfil disponível para calcular o tamanho.";
            _logService.AddWarning("Cálculo não iniciado: selecione ao menos um perfil disponível.");
            UpdateRemoveButtonState();
            return;
        }

        AdminCredentialInfo? credential = null;

        if (chkUseAdminCredential.Checked)
        {
            using var credentialForm = new CredentialForm();
            if (credentialForm.ShowDialog(this) != DialogResult.OK || credentialForm.Credential is null)
            {
                _logService.AddWarning("Credencial administrativa cancelada. Cálculo dos perfis selecionados cancelado.");
                lblStatus.Text = "cálculo cancelado.";
                return;
            }

            credential = credentialForm.Credential;
        }

        _logService.AddInfo($"Iniciando cálculo de tamanho para {selectedProfiles.Count} perfil(is) selecionado(s).");
        foreach (var profile in selectedProfiles)
        {
            _logService.AddInfo($"Perfil selecionado para cálculo: {profile}.");
        }

        lblStatus.Text = selectedProfiles.Count == 1
            ? "calculando tamanho do perfil selecionado..."
            : $"calculando tamanho de {selectedProfiles.Count} perfis selecionados...";

        SetSizeCalculationState(isCalculating: true);

        _sizeCalculationCancellation?.Dispose();
        _sizeCalculationCancellation = new CancellationTokenSource();
        var results = new Dictionary<string, ProfileSizeResult>(StringComparer.OrdinalIgnoreCase);

        var progress = new Progress<ProfileSizeResult>(result =>
        {
            result.Profile.SizeDisplay = GetIndividualSizeDisplayText(result);
            result.Profile.SizeBytes = result.Bytes;
            if (!string.Equals(result.DisplayText, "Calculando...", StringComparison.OrdinalIgnoreCase))
            {
                results[GetProfileKey(result.Profile)] = result;
            }

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

            if (_sizeCalculationCancellation.IsCancellationRequested)
            {
                lblStatus.Text = "cálculo cancelado.";
                _logService.AddWarning("Cálculo dos perfis selecionados cancelado pelo usuário.");
            }
            else
            {
                var calculatedCount = results.Values.Count(result => result.Status == ProfileSizeResultStatus.Calculated);
                var notApplicableCount = results.Values.Count(result => result.Status == ProfileSizeResultStatus.Ignored);
                var errorCount = results.Values.Count(result =>
                    result.Status is ProfileSizeResultStatus.AccessDenied
                        or ProfileSizeResultStatus.Timeout
                        or ProfileSizeResultStatus.Error);

                lblStatus.Text = $"cálculo concluído | {calculatedCount} calculado(s) | {notApplicableCount} não aplicável(is) | {errorCount} erro(s).";
                _logService.AddInfo($"Cálculo dos perfis selecionados concluído: {calculatedCount} calculado(s), {notApplicableCount} não aplicável(is), {errorCount} erro(s).");
            }
        }
        catch (OperationCanceledException)
        {
            foreach (var profile in selectedProfiles.Where(profile => profile.SizeDisplay is "Não calculado" or "Calculando..."))
            {
                profile.SizeDisplay = "Cancelado";
                profile.SizeBytes = null;
            }

            _logService.AddWarning("Cálculo dos perfis selecionados cancelado pelo usuário.");
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
        chkSelectAllRemovable.Enabled = !isCalculating && HasVisibleRemovableProfiles();
        txtComputerName.Enabled = !isCalculating;
        chkUseAdminCredential.Enabled = !isCalculating;
        chkCalculateProfileSize.Enabled = !isCalculating;
        chkShowAdvancedSettings.Enabled = !isCalculating;
        chkShowSystemProfiles.Enabled = !isCalculating;
        chkShowTechnicalDetails.Enabled = !isCalculating;
        SetBusyCursor(isCalculating);
    }

    private void SetBusyCursor(bool isBusy)
    {
        UseWaitCursor = isBusy;
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
        Cursor.Current = isBusy ? Cursors.WaitCursor : Cursors.Default;

        if (dgvProfiles is not null)
        {
            dgvProfiles.Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
        }
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
        ApplyDefaultColumnWidths();
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
        LayoutResponsiveSections();
    }

    private void ApplyTechnicalColumnsVisibility()
    {
        var showTechnicalDetails = chkShowTechnicalDetails.Checked;
        colSid.Visible = showTechnicalDetails;
        colLocalPath.Visible = showTechnicalDetails;
        colOperationStatus.Visible = showTechnicalDetails;
        colObservation.Visible = showTechnicalDetails;
        ApplyDefaultColumnWidths();
        ApplySortGlyph();
    }

    private void ApplyDefaultColumnWidths()
    {
        if (chkShowTechnicalDetails.Checked)
        {
            ApplyAdvancedColumnLayout();
        }
        else
        {
            ApplySimpleColumnLayout();
        }

        colSelection.Width = 70;
        colUserName.Width = 180;
        colLastUseTime.Width = 150;
        colIsLoaded.Width = 80;
        colSize.Width = 120;

        colSelection.MinimumWidth = 70;
        colUserName.MinimumWidth = 180;
        colLastUseTime.MinimumWidth = 150;
        colIsLoaded.MinimumWidth = 80;
        colSize.MinimumWidth = 120;

        AutoFitProfileGridColumns();

        colSelection.Frozen = false;
        colUserName.Frozen = false;
    }

    private void ApplySimpleColumnLayout()
    {
        dgvProfiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        dgvProfiles.ScrollBars = ScrollBars.Both;

        colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colStatus.MinimumWidth = 220;
        colStatus.FillWeight = 100F;
    }

    private void ApplyAdvancedColumnLayout()
    {
        dgvProfiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        dgvProfiles.ScrollBars = ScrollBars.Both;

        colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        colStatus.MinimumWidth = 220;

        colSid.MinimumWidth = 260;
        colLocalPath.MinimumWidth = 220;
        colOperationStatus.MinimumWidth = 140;
        colObservation.MinimumWidth = 160;
    }

    private void AutoFitProfileGridColumns()
    {
        if (chkShowTechnicalDetails.Checked)
        {
            AutoFitColumnWithinBounds(colUserName, minWidth: 140, maxWidth: 240);
            AutoFitColumnWithinBounds(colLastUseTime, minWidth: 140, maxWidth: 160);
            AutoFitColumnWithinBounds(colIsLoaded, minWidth: 70, maxWidth: 90);
            AutoFitColumnWithinBounds(colSize, minWidth: 110, maxWidth: 140);
            AutoFitColumnWithinBounds(colStatus, minWidth: 220, maxWidth: 320);
            AutoFitColumnWithinBounds(colSid, minWidth: 260, maxWidth: 360);
            AutoFitColumnWithinBounds(colLocalPath, minWidth: 220, maxWidth: 360);
            AutoFitColumnWithinBounds(colOperationStatus, minWidth: 140, maxWidth: 220);
            AutoFitColumnWithinBounds(colObservation, minWidth: 160, maxWidth: 260);
            return;
        }

        AutoFitColumnWithinBounds(colUserName, minWidth: 140, maxWidth: 240);
        AutoFitColumnWithinBounds(colLastUseTime, minWidth: 140, maxWidth: 150);
        AutoFitColumnWithinBounds(colIsLoaded, minWidth: 80, maxWidth: 80);
        AutoFitColumnWithinBounds(colSize, minWidth: 110, maxWidth: 130);
    }

    private void AutoFitColumnWithinBounds(DataGridViewColumn column, int minWidth, int maxWidth)
    {
        if (!column.Visible)
        {
            return;
        }

        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        dgvProfiles.AutoResizeColumn(column.Index, DataGridViewAutoSizeColumnMode.DisplayedCells);
        column.Width = Math.Clamp(column.Width, minWidth, maxWidth);
    }

    private void ResizeMainSections()
    {
        if (txtLogs is null || dgvProfiles is null)
        {
            return;
        }

        var contentWidth = Math.Max(0, ClientSize.Width - (SideMargin * 2));
        var logsTop = ClientSize.Height - SideMargin - LogsHeight;
        var logsHeaderTop = logsTop - LogsHeaderToTextGap - LogsHeaderHeight;
        var gridBottom = logsHeaderTop - LogsGap;

        lblLogs.Left = SideMargin;
        dgvProfiles.Height = Math.Max(180, gridBottom - dgvProfiles.Top);
        lblLogs.Top = logsHeaderTop + 7;
        btnCopyLog.Left = ClientSize.Width - SideMargin - btnCopyLog.Width;
        btnClearLog.Left = btnCopyLog.Left - 8 - btnClearLog.Width;
        btnClearLog.Top = logsHeaderTop;
        btnCopyLog.Top = logsHeaderTop;
        txtLogs.Left = SideMargin;
        txtLogs.Width = contentWidth;
        txtLogs.Top = logsTop;
        txtLogs.Height = LogsHeight;
        lblAuthor.Left = ClientSize.Width - SideMargin - lblAuthor.Width;
        lblAuthor.Top = txtLogs.Bottom + 6;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutResponsiveSections();
    }

    private void LayoutResponsiveSections()
    {
        if (grpConnection is null || grpOptions is null || grpActions is null)
        {
            return;
        }

        var contentWidth = Math.Max(0, ClientSize.Width - (SideMargin * 2));
        var availableTopWidth = Math.Max(0, contentWidth - (TopGroupsGap * 2));

        var connectionWidth = Math.Max(360, (int)(availableTopWidth * 0.35));
        var optionsWidth = Math.Max(320, (int)(availableTopWidth * 0.31));
        var actionsWidth = availableTopWidth - connectionWidth - optionsWidth;

        if (actionsWidth < 328)
        {
            var deficit = 328 - actionsWidth;
            var reduceConnection = Math.Min(deficit / 2, Math.Max(0, connectionWidth - 360));
            connectionWidth -= reduceConnection;
            deficit -= reduceConnection;

            var reduceOptions = Math.Min(deficit, Math.Max(0, optionsWidth - 320));
            optionsWidth -= reduceOptions;
            actionsWidth = availableTopWidth - connectionWidth - optionsWidth;
        }

        actionsWidth = Math.Max(328, actionsWidth);

        grpConnection.SetBounds(SideMargin, TopGroupsTop, connectionWidth, TopGroupsHeight);
        grpOptions.SetBounds(grpConnection.Right + TopGroupsGap, TopGroupsTop, optionsWidth, TopGroupsHeight);
        grpActions.SetBounds(grpOptions.Right + TopGroupsGap, TopGroupsTop, Math.Max(328, SideMargin + contentWidth - (grpOptions.Right + TopGroupsGap)), TopGroupsHeight);

        grpAdvancedSettings.SetBounds(SideMargin, 232, contentWidth, 52);

        var statusTop = chkShowAdvancedSettings.Checked ? ExpandedStatusTop : CollapsedStatusTop;
        grpStatus.SetBounds(SideMargin, statusTop, contentWidth, 50);
        lblStatus.Width = Math.Max(120, grpStatus.Width - lblStatus.Left - GroupPadding);

        lblLegend.SetBounds(SideMargin, grpStatus.Bottom + LegendGap, contentWidth, 20);
        chkSelectAllRemovable.Left = SideMargin;
        chkSelectAllRemovable.Top = lblLegend.Bottom + GridGap;
        dgvProfiles.Left = SideMargin;
        dgvProfiles.Top = chkSelectAllRemovable.Bottom + GridGap;
        dgvProfiles.Width = contentWidth;

        LayoutConnectionGroup();
        LayoutOptionsGroup();
        LayoutActionsGroup();
        LayoutAdvancedSettingsGroup();
        ResizeMainSections();
    }

    private void LayoutConnectionGroup()
    {
        var buttonWidth = 112;
        var maxTextWidth = 360;
        var availableWidth = Math.Max(120, grpConnection.Width - (GroupPadding * 2) - ButtonGap - buttonWidth);
        var textWidth = Math.Min(maxTextWidth, availableWidth);

        lblComputerName.Left = GroupPadding;
        txtComputerName.SetBounds(GroupPadding, 53, textWidth, txtComputerName.Height);
        btnLoadProfiles.SetBounds(txtComputerName.Right + ButtonGap, 52, buttonWidth, btnLoadProfiles.Height);
    }

    private void LayoutOptionsGroup()
    {
        chkCalculateProfileSize.Left = GroupPadding;
        chkShowAdvancedSettings.Left = GroupPadding;
    }

    private void LayoutActionsGroup()
    {
        var availableWidth = Math.Max(260, grpActions.Width - (GroupPadding * 2));
        var firstRowButtonWidth = Math.Min(ActionButtonMaxWidth, Math.Max(120, (availableWidth - ButtonGap) / 2));
        var calculateButtonWidth = Math.Min(CalculateButtonMaxWidth, Math.Max(firstRowButtonWidth, (firstRowButtonWidth * 2) + ButtonGap));

        btnRemoveSelected.SetBounds(GroupPadding, 27, firstRowButtonWidth, ActionButtonHeight);
        btnCancelSizeCalculation.SetBounds(btnRemoveSelected.Right + ButtonGap, 27, firstRowButtonWidth, ActionButtonHeight);
        btnCalculateSelectedSize.SetBounds(GroupPadding, 66, calculateButtonWidth, ActionButtonHeight);
    }

    private void LayoutAdvancedSettingsGroup()
    {
        var availableWidth = Math.Max(0, grpAdvancedSettings.Width - (GroupPadding * 2));
        var columnWidth = Math.Max(220, availableWidth / 3);

        chkUseAdminCredential.Left = GroupPadding;
        chkShowSystemProfiles.Left = GroupPadding + columnWidth;
        chkShowTechnicalDetails.Left = GroupPadding + (columnWidth * 2);
    }

    private static string GetFriendlyRemovalStatus(string message)
    {
        return string.Equals(message, "Removido", StringComparison.OrdinalIgnoreCase)
            ? "Removido com sucesso"
            : message;
    }
}
