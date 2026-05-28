using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Forms;

public partial class CredentialForm : Form
{
    public CredentialForm()
    {
        InitializeComponent();
    }

    public AdminCredentialInfo? Credential { get; private set; }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (!CredentialHelper.TryCreate(txtUserName.Text, txtPassword.Text, out var credential, out var validationMessage))
        {
            MessageBox.Show(
                validationMessage,
                "Credencial administrativa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        Credential = credential;
        txtPassword.Clear();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        txtPassword.Clear();
        Credential = null;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        txtPassword.Clear();
        base.OnFormClosed(e);
    }
}
