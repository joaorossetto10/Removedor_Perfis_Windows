namespace RemovedorPerfisWindows.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Button btnThemeToggle;
    private Label lblDescription;
    private Label lblStepsHelp;
    private GroupBox grpConnection;
    private Label lblComputerName;
    private TextBox txtComputerName;
    private Button btnLoadProfiles;
    private GroupBox grpOptions;
    private CheckBox chkCalculateProfileSize;
    private CheckBox chkShowAdvancedSettings;
    private GroupBox grpAdvancedSettings;
    private CheckBox chkUseAdminCredential;
    private CheckBox chkShowSystemProfiles;
    private CheckBox chkShowTechnicalDetails;
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
    private Label lblAuthor;
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
        btnThemeToggle = new Button();
        lblDescription = new Label();
        lblStepsHelp = new Label();
        grpConnection = new GroupBox();
        lblComputerName = new Label();
        txtComputerName = new TextBox();
        btnLoadProfiles = new Button();
        grpOptions = new GroupBox();
        chkCalculateProfileSize = new CheckBox();
        chkShowAdvancedSettings = new CheckBox();
        grpAdvancedSettings = new GroupBox();
        chkUseAdminCredential = new CheckBox();
        chkShowSystemProfiles = new CheckBox();
        chkShowTechnicalDetails = new CheckBox();
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
        lblAuthor = new Label();
        txtLogs = new TextBox();
        toolTip = new ToolTip(components);
        grpConnection.SuspendLayout();
        grpOptions.SuspendLayout();
        grpAdvancedSettings.SuspendLayout();
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
        // btnThemeToggle
        // 
        btnThemeToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnThemeToggle.FlatStyle = FlatStyle.Flat;
        btnThemeToggle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnThemeToggle.Location = new Point(1032, 18);
        btnThemeToggle.Name = "btnThemeToggle";
        btnThemeToggle.Size = new Size(36, 30);
        btnThemeToggle.TabIndex = 14;
        btnThemeToggle.Text = "☾";
        toolTip.SetToolTip(btnThemeToggle, "Alternar modo claro/escuro.");
        btnThemeToggle.UseVisualStyleBackColor = true;
        btnThemeToggle.Click += BtnThemeToggle_Click;
        // 
        // lblDescription
        // 
        lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblDescription.Location = new Point(28, 55);
        lblDescription.Name = "lblDescription";
        lblDescription.Size = new Size(1040, 20);
        lblDescription.TabIndex = 1;
        lblDescription.Text = "Ferramenta interna de TI para gerenciamento seguro de perfis locais do Windows.";
        // 
        // lblStepsHelp
        // 
        lblStepsHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStepsHelp.Location = new Point(28, 77);
        lblStepsHelp.Name = "lblStepsHelp";
        lblStepsHelp.Size = new Size(1040, 34);
        lblStepsHelp.TabIndex = 2;
        lblStepsHelp.Text = "Passo 1: informe o computador e carregue os perfis.  Passo 2: analise perfil, último uso, tamanho e status.  Passo 3: selecione apenas perfis disponíveis e remova com confirmação.";
        // 
        // grpConnection
        // 
        grpConnection.Controls.Add(lblComputerName);
        grpConnection.Controls.Add(txtComputerName);
        grpConnection.Controls.Add(btnLoadProfiles);
        grpConnection.Location = new Point(28, 120);
        grpConnection.Name = "grpConnection";
        grpConnection.Size = new Size(360, 88);
        grpConnection.TabIndex = 3;
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
        txtComputerName.Size = new Size(210, 23);
        txtComputerName.TabIndex = 1;
        toolTip.SetToolTip(txtComputerName, "Informe o nome ou IP do computador na rede.");
        // 
        // btnLoadProfiles
        // 
        btnLoadProfiles.Location = new Point(238, 44);
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
        grpOptions.Controls.Add(chkShowAdvancedSettings);
        grpOptions.Location = new Point(404, 120);
        grpOptions.Name = "grpOptions";
        grpOptions.Size = new Size(320, 88);
        grpOptions.TabIndex = 4;
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
        toolTip.SetToolTip(chkCalculateProfileSize, "Calcula o espaço usado por cada perfil. Pode demorar em perfis grandes.");
        chkCalculateProfileSize.UseVisualStyleBackColor = true;
        // 
        // chkShowAdvancedSettings
        // 
        chkShowAdvancedSettings.AutoSize = true;
        chkShowAdvancedSettings.Location = new Point(16, 61);
        chkShowAdvancedSettings.Name = "chkShowAdvancedSettings";
        chkShowAdvancedSettings.Size = new Size(203, 19);
        chkShowAdvancedSettings.TabIndex = 1;
        chkShowAdvancedSettings.Text = "Mostrar configurações avançadas";
        toolTip.SetToolTip(chkShowAdvancedSettings, "Use as configurações avançadas apenas se souber o que está fazendo.");
        chkShowAdvancedSettings.UseVisualStyleBackColor = true;
        chkShowAdvancedSettings.CheckedChanged += ChkShowAdvancedSettings_CheckedChanged;
        // 
        // grpAdvancedSettings
        // 
        grpAdvancedSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpAdvancedSettings.Controls.Add(chkUseAdminCredential);
        grpAdvancedSettings.Controls.Add(chkShowSystemProfiles);
        grpAdvancedSettings.Controls.Add(chkShowTechnicalDetails);
        grpAdvancedSettings.Location = new Point(28, 216);
        grpAdvancedSettings.Name = "grpAdvancedSettings";
        grpAdvancedSettings.Size = new Size(1040, 52);
        grpAdvancedSettings.TabIndex = 6;
        grpAdvancedSettings.TabStop = false;
        grpAdvancedSettings.Text = "Configurações avançadas";
        grpAdvancedSettings.Visible = false;
        // 
        // chkUseAdminCredential
        // 
        chkUseAdminCredential.AutoSize = true;
        chkUseAdminCredential.Location = new Point(16, 22);
        chkUseAdminCredential.Name = "chkUseAdminCredential";
        chkUseAdminCredential.Size = new Size(190, 19);
        chkUseAdminCredential.TabIndex = 0;
        chkUseAdminCredential.Text = "Usar credencial administrativa";
        toolTip.SetToolTip(chkUseAdminCredential, "Use apenas se sua conta atual não tiver permissão no computador remoto.");
        chkUseAdminCredential.UseVisualStyleBackColor = true;
        // 
        // chkShowSystemProfiles
        // 
        chkShowSystemProfiles.AutoSize = true;
        chkShowSystemProfiles.Location = new Point(238, 22);
        chkShowSystemProfiles.Name = "chkShowSystemProfiles";
        chkShowSystemProfiles.Size = new Size(210, 19);
        chkShowSystemProfiles.TabIndex = 1;
        chkShowSystemProfiles.Text = "Mostrar perfis de sistema/serviço";
        toolTip.SetToolTip(chkShowSystemProfiles, "Exibe perfis técnicos do Windows. Eles continuam bloqueados.");
        chkShowSystemProfiles.UseVisualStyleBackColor = true;
        chkShowSystemProfiles.CheckedChanged += ChkShowSystemProfiles_CheckedChanged;
        // 
        // chkShowTechnicalDetails
        // 
        chkShowTechnicalDetails.AutoSize = true;
        chkShowTechnicalDetails.Location = new Point(490, 22);
        chkShowTechnicalDetails.Name = "chkShowTechnicalDetails";
        chkShowTechnicalDetails.Size = new Size(153, 19);
        chkShowTechnicalDetails.TabIndex = 2;
        chkShowTechnicalDetails.Text = "Mostrar detalhes técnicos";
        toolTip.SetToolTip(chkShowTechnicalDetails, "Mostra SID, caminho completo e informações avançadas.");
        chkShowTechnicalDetails.UseVisualStyleBackColor = true;
        chkShowTechnicalDetails.CheckedChanged += ChkShowTechnicalDetails_CheckedChanged;
        // 
        // grpActions
        // 
        grpActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpActions.Controls.Add(btnRemoveSelected);
        grpActions.Controls.Add(btnCancelSizeCalculation);
        grpActions.Location = new Point(740, 120);
        grpActions.Name = "grpActions";
        grpActions.Size = new Size(328, 88);
        grpActions.TabIndex = 5;
        grpActions.TabStop = false;
        grpActions.Text = "Ações";
        // 
        // btnRemoveSelected
        // 
        btnRemoveSelected.Enabled = false;
        btnRemoveSelected.BackColor = Color.FromArgb(255, 244, 230);
        btnRemoveSelected.FlatStyle = FlatStyle.Standard;
        btnRemoveSelected.Location = new Point(16, 31);
        btnRemoveSelected.Name = "btnRemoveSelected";
        btnRemoveSelected.Size = new Size(145, 28);
        btnRemoveSelected.TabIndex = 0;
        btnRemoveSelected.Text = "Remover selecionados";
        toolTip.SetToolTip(btnRemoveSelected, "Remove somente perfis locais permitidos e exige confirmação.");
        btnRemoveSelected.UseVisualStyleBackColor = false;
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
        grpStatus.Location = new Point(28, 276);
        grpStatus.Name = "grpStatus";
        grpStatus.Size = new Size(1040, 50);
        grpStatus.TabIndex = 7;
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
        lblLegend.Location = new Point(28, 336);
        lblLegend.Name = "lblLegend";
        lblLegend.Size = new Size(1040, 20);
        lblLegend.TabIndex = 8;
        lblLegend.Text = "Legenda: branco = disponível | azul = selecionado | cinza = protegido/sistema | amarelo = atenção | vermelho = bloqueado/em uso/erro | verde = removido";
        // 
        // dgvProfiles
        // 
        dgvProfiles.AllowUserToAddRows = false;
        dgvProfiles.AllowUserToDeleteRows = false;
        dgvProfiles.AllowUserToResizeRows = false;
        dgvProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvProfiles.BackgroundColor = SystemColors.Window;
        dgvProfiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvProfiles.Columns.AddRange(new DataGridViewColumn[] { colSelection, colUserName, colLastUseTime, colIsLoaded, colSize, colStatus, colSid, colLocalPath, colOperationStatus, colObservation });
        dgvProfiles.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvProfiles.Location = new Point(28, 360);
        dgvProfiles.MultiSelect = false;
        dgvProfiles.Name = "dgvProfiles";
        dgvProfiles.RowHeadersVisible = false;
        dgvProfiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProfiles.Size = new Size(1040, 191);
        dgvProfiles.TabIndex = 9;
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
        colUserName.Width = 170;
        // 
        // colSid
        // 
        colSid.DataPropertyName = "Sid";
        colSid.HeaderText = "SID";
        colSid.Name = "colSid";
        colSid.ReadOnly = true;
        colSid.Visible = false;
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
        colLocalPath.Visible = false;
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
        colOperationStatus.Visible = false;
        colOperationStatus.Width = 115;
        // 
        // colObservation
        // 
        colObservation.DataPropertyName = "Observation";
        colObservation.HeaderText = "Observação";
        colObservation.Name = "colObservation";
        colObservation.ReadOnly = true;
        colObservation.Visible = false;
        colObservation.Width = 120;
        // 
        // lblLogs
        // 
        lblLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblLogs.AutoSize = true;
        lblLogs.Location = new Point(28, 570);
        lblLogs.Name = "lblLogs";
        lblLogs.Size = new Size(31, 15);
        lblLogs.TabIndex = 10;
        lblLogs.Text = "Logs técnicos - use os logs para diagnóstico técnico.";
        // 
        // btnClearLog
        // 
        btnClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClearLog.Location = new Point(900, 563);
        btnClearLog.Name = "btnClearLog";
        btnClearLog.Size = new Size(78, 27);
        btnClearLog.TabIndex = 11;
        btnClearLog.Text = "Limpar log";
        btnClearLog.UseVisualStyleBackColor = true;
        btnClearLog.Click += BtnClearLog_Click;
        // 
        // btnCopyLog
        // 
        btnCopyLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCopyLog.Location = new Point(986, 563);
        btnCopyLog.Name = "btnCopyLog";
        btnCopyLog.Size = new Size(82, 27);
        btnCopyLog.TabIndex = 12;
        btnCopyLog.Text = "Copiar log";
        btnCopyLog.UseVisualStyleBackColor = true;
        btnCopyLog.Click += BtnCopyLog_Click;
        // 
        // lblAuthor
        // 
        lblAuthor.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        lblAuthor.AutoSize = true;
        lblAuthor.Location = new Point(912, 735);
        lblAuthor.Name = "lblAuthor";
        lblAuthor.Size = new Size(156, 15);
        lblAuthor.TabIndex = 15;
        lblAuthor.Text = "by João Vitor Paska Rossetto";
        // 
        // txtLogs
        // 
        txtLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLogs.Font = new Font("Consolas", 9F);
        txtLogs.Location = new Point(28, 601);
        txtLogs.Multiline = true;
        txtLogs.Name = "txtLogs";
        txtLogs.ReadOnly = true;
        txtLogs.ScrollBars = ScrollBars.Vertical;
        txtLogs.Size = new Size(1040, 125);
        txtLogs.TabIndex = 13;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1094, 754);
        Controls.Add(txtLogs);
        Controls.Add(lblAuthor);
        Controls.Add(btnCopyLog);
        Controls.Add(btnClearLog);
        Controls.Add(lblLogs);
        Controls.Add(dgvProfiles);
        Controls.Add(lblLegend);
        Controls.Add(grpStatus);
        Controls.Add(grpAdvancedSettings);
        Controls.Add(grpActions);
        Controls.Add(grpOptions);
        Controls.Add(grpConnection);
        Controls.Add(lblDescription);
        Controls.Add(lblStepsHelp);
        Controls.Add(btnThemeToggle);
        Controls.Add(lblTitle);
        MinimumSize = new Size(1040, 760);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Removedor de Perfis Windows";
        grpConnection.ResumeLayout(false);
        grpConnection.PerformLayout();
        grpOptions.ResumeLayout(false);
        grpOptions.PerformLayout();
        grpAdvancedSettings.ResumeLayout(false);
        grpAdvancedSettings.PerformLayout();
        grpActions.ResumeLayout(false);
        grpStatus.ResumeLayout(false);
        grpStatus.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProfiles).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
