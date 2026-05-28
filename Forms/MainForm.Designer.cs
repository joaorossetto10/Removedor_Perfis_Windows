namespace RemovedorPerfisWindows.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Label lblDescription;
    private GroupBox grpConnection;
    private Label lblComputerName;
    private TextBox txtComputerName;
    private Button btnLoadProfiles;
    private Label lblStatus;
    private DataGridView dgvProfiles;
    private DataGridViewCheckBoxColumn colSelection;
    private DataGridViewTextBoxColumn colUserName;
    private DataGridViewTextBoxColumn colSid;
    private DataGridViewTextBoxColumn colLocalPath;
    private DataGridViewCheckBoxColumn colIsLoaded;
    private DataGridViewTextBoxColumn colLastUseTime;
    private DataGridViewTextBoxColumn colStatus;
    private Label lblLogs;
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
        grpConnection = new GroupBox();
        lblComputerName = new Label();
        txtComputerName = new TextBox();
        btnLoadProfiles = new Button();
        lblStatus = new Label();
        dgvProfiles = new DataGridView();
        colSelection = new DataGridViewCheckBoxColumn();
        colUserName = new DataGridViewTextBoxColumn();
        colSid = new DataGridViewTextBoxColumn();
        colLocalPath = new DataGridViewTextBoxColumn();
        colIsLoaded = new DataGridViewCheckBoxColumn();
        colLastUseTime = new DataGridViewTextBoxColumn();
        colStatus = new DataGridViewTextBoxColumn();
        lblLogs = new Label();
        txtLogs = new TextBox();
        grpConnection.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProfiles).BeginInit();
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
        lblDescription.Size = new Size(930, 38);
        lblDescription.TabIndex = 1;
        lblDescription.Text = "Ferramenta interna de TI para gerenciamento seguro de perfis locais do Windows.";
        // 
        // grpConnection
        // 
        grpConnection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpConnection.Controls.Add(lblComputerName);
        grpConnection.Controls.Add(txtComputerName);
        grpConnection.Controls.Add(btnLoadProfiles);
        grpConnection.Controls.Add(lblStatus);
        grpConnection.Location = new Point(28, 111);
        grpConnection.Name = "grpConnection";
        grpConnection.Size = new Size(930, 90);
        grpConnection.TabIndex = 2;
        grpConnection.TabStop = false;
        grpConnection.Text = "Consulta";
        // 
        // lblComputerName
        // 
        lblComputerName.AutoSize = true;
        lblComputerName.Location = new Point(16, 29);
        lblComputerName.Name = "lblComputerName";
        lblComputerName.Size = new Size(120, 15);
        lblComputerName.TabIndex = 0;
        lblComputerName.Text = "Computador remoto";
        // 
        // txtComputerName
        // 
        txtComputerName.Location = new Point(16, 47);
        txtComputerName.Name = "txtComputerName";
        txtComputerName.Size = new Size(260, 23);
        txtComputerName.TabIndex = 1;
        // 
        // btnLoadProfiles
        // 
        btnLoadProfiles.Location = new Point(292, 46);
        btnLoadProfiles.Name = "btnLoadProfiles";
        btnLoadProfiles.Size = new Size(120, 25);
        btnLoadProfiles.TabIndex = 2;
        btnLoadProfiles.Text = "Carregar perfis";
        btnLoadProfiles.UseVisualStyleBackColor = true;
        btnLoadProfiles.Click += BtnLoadProfiles_Click;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Location = new Point(430, 50);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(482, 20);
        lblStatus.TabIndex = 3;
        lblStatus.Text = "Informe um computador remoto para carregar os perfis locais.";
        // 
        // dgvProfiles
        // 
        dgvProfiles.AllowUserToAddRows = false;
        dgvProfiles.AllowUserToDeleteRows = false;
        dgvProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvProfiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvProfiles.Columns.AddRange(new DataGridViewColumn[] { colSelection, colUserName, colSid, colLocalPath, colIsLoaded, colLastUseTime, colStatus });
        dgvProfiles.Location = new Point(28, 217);
        dgvProfiles.MultiSelect = false;
        dgvProfiles.Name = "dgvProfiles";
        dgvProfiles.RowHeadersVisible = false;
        dgvProfiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProfiles.Size = new Size(930, 240);
        dgvProfiles.TabIndex = 3;
        // 
        // colSelection
        // 
        colSelection.DataPropertyName = "IsSelected";
        colSelection.HeaderText = "Seleção";
        colSelection.Name = "colSelection";
        colSelection.Width = 70;
        // 
        // colUserName
        // 
        colUserName.DataPropertyName = "UserName";
        colUserName.HeaderText = "Nome do perfil";
        colUserName.Name = "colUserName";
        colUserName.ReadOnly = true;
        colUserName.Width = 140;
        // 
        // colSid
        // 
        colSid.DataPropertyName = "Sid";
        colSid.HeaderText = "SID";
        colSid.Name = "colSid";
        colSid.ReadOnly = true;
        colSid.Width = 190;
        // 
        // colLocalPath
        // 
        colLocalPath.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colLocalPath.DataPropertyName = "LocalPath";
        colLocalPath.HeaderText = "Caminho local";
        colLocalPath.Name = "colLocalPath";
        colLocalPath.ReadOnly = true;
        // 
        // colIsLoaded
        // 
        colIsLoaded.DataPropertyName = "IsLoaded";
        colIsLoaded.HeaderText = "Em uso";
        colIsLoaded.Name = "colIsLoaded";
        colIsLoaded.ReadOnly = true;
        colIsLoaded.Width = 70;
        // 
        // colLastUseTime
        // 
        colLastUseTime.DataPropertyName = "LastUseTimeText";
        colLastUseTime.HeaderText = "Último uso";
        colLastUseTime.Name = "colLastUseTime";
        colLastUseTime.ReadOnly = true;
        colLastUseTime.Width = 145;
        // 
        // colStatus
        // 
        colStatus.DataPropertyName = "Status";
        colStatus.HeaderText = "Status";
        colStatus.Name = "colStatus";
        colStatus.ReadOnly = true;
        colStatus.Width = 150;
        // 
        // lblLogs
        // 
        lblLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblLogs.AutoSize = true;
        lblLogs.Location = new Point(28, 473);
        lblLogs.Name = "lblLogs";
        lblLogs.Size = new Size(31, 15);
        lblLogs.TabIndex = 4;
        lblLogs.Text = "Logs";
        // 
        // txtLogs
        // 
        txtLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLogs.Location = new Point(28, 493);
        txtLogs.Multiline = true;
        txtLogs.Name = "txtLogs";
        txtLogs.ReadOnly = true;
        txtLogs.ScrollBars = ScrollBars.Vertical;
        txtLogs.Size = new Size(930, 118);
        txtLogs.TabIndex = 5;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(984, 631);
        Controls.Add(txtLogs);
        Controls.Add(lblLogs);
        Controls.Add(dgvProfiles);
        Controls.Add(grpConnection);
        Controls.Add(lblDescription);
        Controls.Add(lblTitle);
        MinimumSize = new Size(900, 560);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Removedor de Perfis Windows";
        grpConnection.ResumeLayout(false);
        grpConnection.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProfiles).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
