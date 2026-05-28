namespace RemovedorPerfisWindows.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Label lblDescription;
    private GroupBox grpInitialStatus;
    private Label lblStatus;
    private TextBox txtLogs;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logService.EntryAdded -= OnLogEntryAdded;
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblDescription = new Label();
        grpInitialStatus = new GroupBox();
        lblStatus = new Label();
        txtLogs = new TextBox();
        grpInitialStatus.SuspendLayout();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.Location = new Point(24, 22);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(331, 30);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Removedor de Perfis Windows";
        // 
        // lblDescription
        // 
        lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblDescription.Location = new Point(28, 65);
        lblDescription.Name = "lblDescription";
        lblDescription.Size = new Size(730, 42);
        lblDescription.TabIndex = 1;
        lblDescription.Text = "Ferramenta interna de TI para gerenciamento seguro de perfis locais do Windows. Nesta primeira etapa, a tela inicial está preparada apenas para estruturação do projeto.";
        // 
        // grpInitialStatus
        // 
        grpInitialStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpInitialStatus.Controls.Add(lblStatus);
        grpInitialStatus.Location = new Point(28, 123);
        grpInitialStatus.Name = "grpInitialStatus";
        grpInitialStatus.Size = new Size(730, 92);
        grpInitialStatus.TabIndex = 2;
        grpInitialStatus.TabStop = false;
        grpInitialStatus.Text = "Status inicial";
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Location = new Point(16, 27);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(696, 48);
        lblStatus.TabIndex = 0;
        lblStatus.Text = "Listagem remota, conexão WMI/CIM e remoção de perfis ainda não estão implementadas.";
        // 
        // txtLogs
        // 
        txtLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLogs.Location = new Point(28, 235);
        txtLogs.Multiline = true;
        txtLogs.Name = "txtLogs";
        txtLogs.ReadOnly = true;
        txtLogs.ScrollBars = ScrollBars.Vertical;
        txtLogs.Size = new Size(730, 175);
        txtLogs.TabIndex = 3;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 441);
        Controls.Add(txtLogs);
        Controls.Add(grpInitialStatus);
        Controls.Add(lblDescription);
        Controls.Add(lblTitle);
        MinimumSize = new Size(720, 420);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Removedor de Perfis Windows";
        grpInitialStatus.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
