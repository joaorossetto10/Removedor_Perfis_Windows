using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Forms;

public partial class RemovalConfirmationForm : Form
{
    private readonly string _computerName;

    public RemovalConfirmationForm(string computerName, IReadOnlyList<UserProfileInfo> profiles)
    {
        _computerName = computerName;
        InitializeComponent();
        lblComputerName.Text = $"Computador remoto: {computerName}";
        lblProfileCount.Text = $"Perfis selecionados: {profiles.Count}";

        foreach (var profile in profiles)
        {
            lstProfiles.Items.Add($"{profile.UserName} | {profile.Sid} | {profile.LocalPath}");
        }
    }

    private void TxtConfirmation_TextChanged(object? sender, EventArgs e)
    {
        btnConfirm.Enabled = string.Equals(
            txtConfirmation.Text.Trim(),
            _computerName,
            StringComparison.OrdinalIgnoreCase);
    }

    private void BtnConfirm_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
