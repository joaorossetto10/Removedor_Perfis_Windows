namespace RemovedorPerfisWindows.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Label lblDescription;
    private GroupBox grpConnection;
    private Label lblComputerName;
    private TextBox txtComputerName;
    private CheckBox chkUseAdminCredential;
    private Button btnLoadProfiles;
    private GroupBox grpOptions;
    private CheckBox chkCalculateProfileSize;
    private CheckBox chkShowSystemProfiles;
    private GroupBox grpActions;
    private Button btnRemoveSelected;
    private Button btnCancelSizeCalculation;
    private GroupBox grpStatus;
    private Label lblStatusTitle;
    private Label lblStatus;
    private Label lblLegend;
    private DataGridView dgvProfiles;
    private DataGridViewCheckBoxColumn colSelection;
    private DataGridViewTextBoxColumn colUserName;
    private DataGridViewTextBoxColumn colSid;
    private DataGridViewTextBoxColumn colLocalPath;
    private DataGridViewCheckBoxColumn colIsLoaded;
    private DataGridViewTextBoxColumn colLastUseTime;
    private DataGridViewTextBoxColumn colStatus;
    private DataGridViewTextBoxColumn colSize;
    private DataGridViewTextBoxColumn colOperationStatus;
    private DataGridViewTextBoxColumn colObservation;
    private Label lblLogs;
    private Button btnClearLog;
    private Button btnCopyLog;
    private TextBox txtLogs;
    private ToolTip toolTip;

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
        components = new System.ComponentModel.Container();
        lblTitle = new Label();
        lblDescription = new Label();
        grpConnection = new GroupBox();
        lblComputerName = new Label();
        txtComputerName = new TextBox();
        chkUseAdminCredential = new CheckBox();
        btnLoadProfiles = new Button();
        grpOptions = new GroupBox();
        chkCalculateProfileSize = new CheckBox();
        chkShowSystemProfiles = new CheckBox();
        grpActions = new GroupBox();
        btnRemoveSelected = new Button();
        btnCancelSizeCalculation = new Button();
        grpStatus = new GroupBox();
        lblStatusTitle = new Label();
        lblStatus = new Label();
        lblLegend = new Label();
        dgvProfiles = new DataGridView();
        colSelection = new DataGridViewCheckBoxColumn();
        colUserName = new DataGridViewTextBoxColumn();
        colSid = new DataGridViewTextBoxColumn();
        colLocalPath = new DataGridViewTextBoxColumn();
        colIsLoaded = new DataGridViewCheckBoxColumn();
        colLastUseTime = new DataGridViewTextBoxColumn();
        colStatus = new DataGridViewTextBoxColumn();
        colSize = new DataGridViewTextBoxColumn();
        colOperationStatus = new DataGridViewTextBoxColumn();
        colObservation = new DataGridViewTextBoxColumn();
        lblLogs = new Label();
        btnClearLog = new Button();
        btnCopyLog = new Button();
        txtLogs = new TextBox();
        toolTip = new ToolTip(components);
        grpConnection.SuspendLayout();
        grpOptions.SuspendLayout();
        grpActions.SuspendLayout();
        grpStatus.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProfiles).BeginInit();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.Location = new Point(24, 18);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(331, 30);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Removedor de Perfis Windows";
        // 
        // lblDescription
        // 
        lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblDescription.Location = new Point(28, 55);
        lblDescription.Name = "lblDescription";
        lblDescription.Size = new Size(1040, 28);
        lblDescription.TabIndex = 1;
        lblDescription.Text = "Ferramenta interna de TI para gerenciamento seguro de perfis locais do Windows.";
        // 
        // grpConnection
        // 
        grpConnection.Controls.Add(lblComputerName);
        grpConnection.Controls.Add(txtComputerName);
        grpConnection.Controls.Add(chkUseAdminCredential);
        grpConnection.Controls.Add(btnLoadProfiles);
        grpConnection.Location = new Point(28, 92);
        grpConnection.Name = "grpConnection";
        grpConnection.Size = new Size(360, 104);
        grpConnection.TabIndex = 2;
        grpConnection.TabStop = false;
        grpConnection.Text = "Conexão";
        // 
        // lblComputerName
        // 
        lblComputerName.AutoSize = true;
        lblComputerName.Location = new Point(16, 26);
        lblComputerName.Name = "lblComputerName";
        lblComputerName.Size = new Size(120, 15);
        lblComputerName.TabIndex = 0;
        lblComputerName.Text = "Computador remoto";
        // 
        // txtComputerName
        // 
        txtComputerName.Location = new Point(16, 45);
        txtComputerName.Name = "txtComputerName";
        txtComputerName.Size = new Size(205, 23);
        txtComputerName.TabIndex = 1;
        // 
        // chkUseAdminCredential
        // 
        chkUseAdminCredential.AutoSize = true;
        chkUseAdminCredential.Location = new Point(16, 75);
        chkUseAdminCredential.Name = "chkUseAdminCredential";
        chkUseAdminCredential.Size = new Size(190, 19);
        chkUseAdminCredential.TabIndex = 3;
        chkUseAdminCredential.Text = "Usar credencial administrativa";
        chkUseAdminCredential.UseVisualStyleBackColor = true;
        // 
        // btnLoadProfiles
        // 
        btnLoadProfiles.Location = new Point(231, 44);
        btnLoadProfiles.Name = "btnLoadProfiles";
        btnLoadProfiles.Size = new Size(112, 25);
        btnLoadProfiles.TabIndex = 2;
        btnLoadProfiles.Text = "Carregar perfis";
        btnLoadProfiles.UseVisualStyleBackColor = true;
        btnLoadProfiles.Click += BtnLoadProfiles_Click;
        // 
        // grpOptions
        // 
        grpOptions.Controls.Add(chkCalculateProfileSize);
        grpOptions.Controls.Add(chkShowSystemProfiles);
        grpOptions.Location = new Point(404, 92);
        grpOptions.Name = "grpOptions";
        grpOptions.Size = new Size(320, 104);
        grpOptions.TabIndex = 3;
        grpOptions.TabStop = false;
        grpOptions.Text = "Opções";
        // 
        // chkCalculateProfileSize
        // 
        chkCalculateProfileSize.AutoSize = true;
        chkCalculateProfileSize.Location = new Point(16, 31);
        chkCalculateProfileSize.Name = "chkCalculateProfileSize";
        chkCalculateProfileSize.Size = new Size(169, 19);
        chkCalculateProfileSize.TabIndex = 0;
        chkCalculateProfileSize.Text = "Calcular tamanho dos perfis";
        toolTip.SetToolTip(chkCalculateProfileSize, "Pode demorar em perfis grandes.");
        chkCalculateProfileSize.UseVisualStyleBackColor = true;
        // 
        // chkShowSystemProfiles
        // 
        chkShowSystemProfiles.AutoSize = true;
        chkShowSystemProfiles.Location = new Point(16, 64);
        chkShowSystemProfiles.Name = "chkShowSystemProfiles";
        chkShowSystemProfiles.Size = new Size(210, 19);
        chkShowSystemProfiles.TabIndex = 1;
        chkShowSystemProfiles.Text = "Mostrar perfis de sistema/serviço";
        toolTip.SetToolTip(chkShowSystemProfiles, "Apenas exibe. Esses perfis continuam bloqueados.");
        chkShowSystemProfiles.UseVisualStyleBackColor = true;
        chkShowSystemProfiles.CheckedChanged += ChkShowSystemProfiles_CheckedChanged;
        // 
        // grpActions
        // 
        grpActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpActions.Controls.Add(btnRemoveSelected);
        grpActions.Controls.Add(btnCancelSizeCalculation);
        grpActions.Location = new Point(740, 92);
        grpActions.Name = "grpActions";
        grpActions.Size = new Size(328, 104);
        grpActions.TabIndex = 4;
        grpActions.TabStop = false;
        grpActions.Text = "Ações";
        // 
        // btnRemoveSelected
        // 
        btnRemoveSelected.Enabled = false;
        btnRemoveSelected.Location = new Point(16, 31);
        btnRemoveSelected.Name = "btnRemoveSelected";
        btnRemoveSelected.Size = new Size(145, 28);
        btnRemoveSelected.TabIndex = 0;
        btnRemoveSelected.Text = "Remover selecionados";
        btnRemoveSelected.UseVisualStyleBackColor = true;
        btnRemoveSelected.Click += BtnRemoveSelected_Click;
        // 
        // btnCancelSizeCalculation
        // 
        btnCancelSizeCalculation.Enabled = false;
        btnCancelSizeCalculation.Location = new Point(174, 31);
        btnCancelSizeCalculation.Name = "btnCancelSizeCalculation";
        btnCancelSizeCalculation.Size = new Size(130, 28);
        btnCancelSizeCalculation.TabIndex = 1;
        btnCancelSizeCalculation.Text = "Cancelar cálculo";
        btnCancelSizeCalculation.UseVisualStyleBackColor = true;
        btnCancelSizeCalculation.Click += BtnCancelSizeCalculation_Click;
        // 
        // grpStatus
        // 
        grpStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpStatus.Controls.Add(lblStatusTitle);
        grpStatus.Controls.Add(lblStatus);
        grpStatus.Location = new Point(28, 206);
        grpStatus.Name = "grpStatus";
        grpStatus.Size = new Size(1040, 50);
        grpStatus.TabIndex = 5;
        grpStatus.TabStop = false;
        grpStatus.Text = "Status";
        // 
        // lblStatusTitle
        // 
        lblStatusTitle.AutoSize = true;
        lblStatusTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblStatusTitle.Location = new Point(16, 22);
        lblStatusTitle.Name = "lblStatusTitle";
        lblStatusTitle.Size = new Size(45, 15);
        lblStatusTitle.TabIndex = 0;
        lblStatusTitle.Text = "Status:";
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Location = new Point(68, 22);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(952, 18);
        lblStatus.TabIndex = 1;
        lblStatus.Text = "aguardando consulta.";
        // 
        // lblLegend
        // 
        lblLegend.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblLegend.Location = new Point(28, 266);
        lblLegend.Name = "lblLegend";
        lblLegend.Size = new Size(1040, 20);
        lblLegend.TabIndex = 6;
        lblLegend.Text = "Legenda: branco = disponível para análise | cinza = protegido/sistema | vermelho claro = bloqueado/em uso/erro | verde = removido";
        // 
        // dgvProfiles
        // 
        dgvProfiles.AllowUserToAddRows = false;
        dgvProfiles.AllowUserToDeleteRows = false;
        dgvProfiles.AllowUserToResizeRows = false;
        dgvProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvProfiles.BackgroundColor = SystemColors.Window;
        dgvProfiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvProfiles.Columns.AddRange(new DataGridViewColumn[] { colSelection, colUserName, colSid, colLocalPath, colIsLoaded, colLastUseTime, colStatus, colSize, colOperationStatus, colObservation });
        dgvProfiles.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvProfiles.Location = new Point(28, 290);
        dgvProfiles.MultiSelect = false;
        dgvProfiles.Name = "dgvProfiles";
        dgvProfiles.RowHeadersVisible = false;
        dgvProfiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProfiles.Size = new Size(1040, 260);
        dgvProfiles.TabIndex = 7;
        dgvProfiles.CellBeginEdit += DgvProfiles_CellBeginEdit;
        dgvProfiles.CellContentClick += DgvProfiles_CellContentClick;
        dgvProfiles.CellValueChanged += DgvProfiles_CellValueChanged;
        dgvProfiles.CurrentCellDirtyStateChanged += DgvProfiles_CurrentCellDirtyStateChanged;
        dgvProfiles.DataBindingComplete += DgvProfiles_DataBindingComplete;
        // 
        // colSelection
        // 
        colSelection.DataPropertyName = "IsSelected";
        colSelection.HeaderText = "Seleção";
        colSelection.Name = "colSelection";
        colSelection.Width = 65;
        // 
        // colUserName
        // 
        colUserName.DataPropertyName = "UserName";
        colUserName.HeaderText = "Perfil";
        colUserName.Name = "colUserName";
        colUserName.ReadOnly = true;
        colUserName.Width = 130;
        // 
        // colSid
        // 
        colSid.DataPropertyName = "Sid";
        colSid.HeaderText = "SID";
        colSid.Name = "colSid";
        colSid.ReadOnly = true;
        colSid.Width = 185;
        // 
        // colLocalPath
        // 
        colLocalPath.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colLocalPath.DataPropertyName = "LocalPath";
        colLocalPath.FillWeight = 145F;
        colLocalPath.HeaderText = "Caminho";
        colLocalPath.Name = "colLocalPath";
        colLocalPath.ReadOnly = true;
        // 
        // colIsLoaded
        // 
        colIsLoaded.DataPropertyName = "IsLoaded";
        colIsLoaded.HeaderText = "Em uso";
        colIsLoaded.Name = "colIsLoaded";
        colIsLoaded.ReadOnly = true;
        colIsLoaded.Width = 65;
        // 
        // colLastUseTime
        // 
        colLastUseTime.DataPropertyName = "LastUseTimeText";
        colLastUseTime.HeaderText = "Último uso";
        colLastUseTime.Name = "colLastUseTime";
        colLastUseTime.ReadOnly = true;
        colLastUseTime.Width = 135;
        // 
        // colStatus
        // 
        colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colStatus.DataPropertyName = "Status";
        colStatus.FillWeight = 120F;
        colStatus.HeaderText = "Status";
        colStatus.Name = "colStatus";
        colStatus.ReadOnly = true;
        // 
        // colSize
        // 
        colSize.DataPropertyName = "SizeDisplay";
        colSize.HeaderText = "Tamanho";
        colSize.Name = "colSize";
        colSize.ReadOnly = true;
        colSize.Width = 105;
        // 
        // colOperationStatus
        // 
        colOperationStatus.DataPropertyName = "OperationStatus";
        colOperationStatus.HeaderText = "Resultado";
        colOperationStatus.Name = "colOperationStatus";
        colOperationStatus.ReadOnly = true;
        colOperationStatus.Width = 115;
        // 
        // colObservation
        // 
        colObservation.DataPropertyName = "Observation";
        colObservation.HeaderText = "Observação";
        colObservation.Name = "colObservation";
        colObservation.ReadOnly = true;
        colObservation.Width = 120;
        // 
        // lblLogs
        // 
        lblLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblLogs.AutoSize = true;
        lblLogs.Location = new Point(28, 568);
        lblLogs.Name = "lblLogs";
        lblLogs.Size = new Size(31, 15);
        lblLogs.TabIndex = 8;
        lblLogs.Text = "Logs";
        // 
        // btnClearLog
        // 
        btnClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClearLog.Location = new Point(900, 562);
        btnClearLog.Name = "btnClearLog";
        btnClearLog.Size = new Size(78, 27);
        btnClearLog.TabIndex = 9;
        btnClearLog.Text = "Limpar log";
        btnClearLog.UseVisualStyleBackColor = true;
        btnClearLog.Click += BtnClearLog_Click;
        // 
        // btnCopyLog
        // 
        btnCopyLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCopyLog.Location = new Point(986, 562);
        btnCopyLog.Name = "btnCopyLog";
        btnCopyLog.Size = new Size(82, 27);
        btnCopyLog.TabIndex = 10;
        btnCopyLog.Text = "Copiar log";
        btnCopyLog.UseVisualStyleBackColor = true;
        btnCopyLog.Click += BtnCopyLog_Click;
        // 
        // txtLogs
        // 
        txtLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLogs.Font = new Font("Consolas", 9F);
        txtLogs.Location = new Point(28, 595);
        txtLogs.Multiline = true;
        txtLogs.Name = "txtLogs";
        txtLogs.ReadOnly = true;
        txtLogs.ScrollBars = ScrollBars.Vertical;
        txtLogs.Size = new Size(1040, 135);
        txtLogs.TabIndex = 11;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1094, 754);
        Controls.Add(txtLogs);
        Controls.Add(btnCopyLog);
        Controls.Add(btnClearLog);
        Controls.Add(lblLogs);
        Controls.Add(dgvProfiles);
        Controls.Add(lblLegend);
        Controls.Add(grpStatus);
        Controls.Add(grpActions);
        Controls.Add(grpOptions);
        Controls.Add(grpConnection);
        Controls.Add(lblDescription);
        Controls.Add(lblTitle);
        MinimumSize = new Size(1040, 680);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Removedor de Perfis Windows";
        grpConnection.ResumeLayout(false);
        grpConnection.PerformLayout();
        grpOptions.ResumeLayout(false);
        grpOptions.PerformLayout();
        grpActions.ResumeLayout(false);
        grpStatus.ResumeLayout(false);
        grpStatus.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProfiles).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
