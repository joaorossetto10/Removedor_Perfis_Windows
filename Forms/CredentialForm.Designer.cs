namespace RemovedorPerfisWindows.Forms;

partial class CredentialForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Label lblUserName;
    private TextBox txtUserName;
    private Label lblPassword;
    private TextBox txtPassword;
    private Button btnOk;
    private Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            txtPassword.Clear();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblUserName = new Label();
        txtUserName = new TextBox();
        lblPassword = new Label();
        txtPassword = new TextBox();
        btnOk = new Button();
        btnCancel = new Button();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Location = new Point(16, 15);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(386, 38);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Informe uma credencial com permissão administrativa no computador remoto.";
        // 
        // lblUserName
        // 
        lblUserName.AutoSize = true;
        lblUserName.Location = new Point(16, 65);
        lblUserName.Name = "lblUserName";
        lblUserName.Size = new Size(47, 15);
        lblUserName.TabIndex = 1;
        lblUserName.Text = "Usuário";
        // 
        // txtUserName
        // 
        txtUserName.Location = new Point(16, 83);
        txtUserName.Name = "txtUserName";
        txtUserName.PlaceholderText = "DOMINIO\\usuario, computador\\usuario ou usuario@dominio.local";
        txtUserName.Size = new Size(386, 23);
        txtUserName.TabIndex = 2;
        // 
        // lblPassword
        // 
        lblPassword.AutoSize = true;
        lblPassword.Location = new Point(16, 121);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(39, 15);
        lblPassword.TabIndex = 3;
        lblPassword.Text = "Senha";
        // 
        // txtPassword
        // 
        txtPassword.Location = new Point(16, 139);
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new Size(386, 23);
        txtPassword.TabIndex = 4;
        txtPassword.UseSystemPasswordChar = true;
        // 
        // btnOk
        // 
        btnOk.Location = new Point(246, 181);
        btnOk.Name = "btnOk";
        btnOk.Size = new Size(75, 27);
        btnOk.TabIndex = 5;
        btnOk.Text = "OK";
        btnOk.UseVisualStyleBackColor = true;
        btnOk.Click += BtnOk_Click;
        // 
        // btnCancel
        // 
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(327, 181);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(75, 27);
        btnCancel.TabIndex = 6;
        btnCancel.Text = "Cancelar";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += BtnCancel_Click;
        // 
        // CredentialForm
        // 
        AcceptButton = btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(418, 224);
        Controls.Add(btnCancel);
        Controls.Add(btnOk);
        Controls.Add(txtPassword);
        Controls.Add(lblPassword);
        Controls.Add(txtUserName);
        Controls.Add(lblUserName);
        Controls.Add(lblTitle);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "CredentialForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Credencial administrativa";
        ResumeLayout(false);
        PerformLayout();
    }
}
