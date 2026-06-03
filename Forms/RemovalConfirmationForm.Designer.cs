namespace RemovedorPerfisWindows.Forms;

partial class RemovalConfirmationForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Label lblWarning;
    private Label lblComputerName;
    private Label lblProfileCount;
    private ListBox lstProfiles;
    private Label lblConfirmation;
    private TextBox txtConfirmation;
    private Button btnConfirm;
    private Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblWarning = new Label();
        lblComputerName = new Label();
        lblProfileCount = new Label();
        lstProfiles = new ListBox();
        lblConfirmation = new Label();
        txtConfirmation = new TextBox();
        btnConfirm = new RemovedorPerfisWindows.Controls.ThemedButton();
        btnCancel = new RemovedorPerfisWindows.Controls.ThemedButton();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.Location = new Point(16, 14);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(240, 21);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Confirmar remoção de perfis";
        // 
        // lblWarning
        // 
        lblWarning.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblWarning.Location = new Point(16, 48);
        lblWarning.Name = "lblWarning";
        lblWarning.Size = new Size(650, 64);
        lblWarning.TabIndex = 1;
        lblWarning.Text = "A remoção de perfil local remove os dados locais desse usuário neste computador. Esta ação não exclui a conta do domínio/AD, mas pode remover arquivos locais do perfil. Confirme apenas se tiver certeza.";
        // 
        // lblComputerName
        // 
        lblComputerName.AutoSize = true;
        lblComputerName.Location = new Point(16, 123);
        lblComputerName.Name = "lblComputerName";
        lblComputerName.Size = new Size(120, 15);
        lblComputerName.TabIndex = 2;
        lblComputerName.Text = "Computador remoto:";
        // 
        // lblProfileCount
        // 
        lblProfileCount.AutoSize = true;
        lblProfileCount.Location = new Point(16, 148);
        lblProfileCount.Name = "lblProfileCount";
        lblProfileCount.Size = new Size(106, 15);
        lblProfileCount.TabIndex = 3;
        lblProfileCount.Text = "Perfis selecionados:";
        // 
        // lstProfiles
        // 
        lstProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lstProfiles.FormattingEnabled = true;
        lstProfiles.ItemHeight = 15;
        lstProfiles.Location = new Point(16, 176);
        lstProfiles.Name = "lstProfiles";
        lstProfiles.Size = new Size(650, 139);
        lstProfiles.TabIndex = 4;
        // 
        // lblConfirmation
        // 
        lblConfirmation.AutoSize = true;
        lblConfirmation.Location = new Point(16, 331);
        lblConfirmation.Name = "lblConfirmation";
        lblConfirmation.Size = new Size(351, 15);
        lblConfirmation.TabIndex = 5;
        lblConfirmation.Text = "Digite o nome do computador remoto para confirmar a remoção:";
        // 
        // txtConfirmation
        // 
        txtConfirmation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtConfirmation.Location = new Point(16, 353);
        txtConfirmation.Name = "txtConfirmation";
        txtConfirmation.Size = new Size(650, 23);
        txtConfirmation.TabIndex = 6;
        txtConfirmation.TextChanged += TxtConfirmation_TextChanged;
        // 
        // btnConfirm
        // 
        btnConfirm.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnConfirm.Enabled = false;
        btnConfirm.Location = new Point(500, 394);
        btnConfirm.Name = "btnConfirm";
        btnConfirm.Size = new Size(82, 27);
        btnConfirm.TabIndex = 7;
        btnConfirm.Text = "Remover";
        btnConfirm.UseVisualStyleBackColor = true;
        btnConfirm.Click += BtnConfirm_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(588, 394);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(78, 27);
        btnCancel.TabIndex = 8;
        btnCancel.Text = "Cancelar";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += BtnCancel_Click;
        // 
        // RemovalConfirmationForm
        // 
        AcceptButton = btnConfirm;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(682, 438);
        Controls.Add(btnCancel);
        Controls.Add(btnConfirm);
        Controls.Add(txtConfirmation);
        Controls.Add(lblConfirmation);
        Controls.Add(lstProfiles);
        Controls.Add(lblProfileCount);
        Controls.Add(lblComputerName);
        Controls.Add(lblWarning);
        Controls.Add(lblTitle);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "RemovalConfirmationForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Confirmar remoção";
        ResumeLayout(false);
        PerformLayout();
    }
}
