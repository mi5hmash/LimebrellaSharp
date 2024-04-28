namespace LimebrellaSharp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            TBSteamIdInput = new TextBox();
            TBFilepath = new TextBox();
            ButtonSelectDir = new Button();
            ButtonUnpackAll = new Button();
            ButtonPackAll = new Button();
            pb_AppIcon = new PictureBox();
            LabelFilepath = new Label();
            LabelSteamIdLeft1 = new Label();
            LabelSteamIdRight1 = new Label();
            TBSteamIdOutput = new TextBox();
            LabelSteamIdLeft2 = new Label();
            LabelSteamIdRight2 = new Label();
            ButtonResignAll = new Button();
            statusStrip1 = new StatusStrip();
            toolStripProgressBar1 = new ToolStripProgressBar();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            versionLabel = new Label();
            authorLabel = new Label();
            toolTip1 = new ToolTip(components);
            ButtonInterchange = new Button();
            ButtonBruteforceSteamId = new Button();
            ButtonOpenOutputDir = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            ButtonAbort = new Button();
            superUserTimer = new System.Windows.Forms.Timer(components);
            superUserTrigger = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pb_AppIcon).BeginInit();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)superUserTrigger).BeginInit();
            SuspendLayout();
            // 
            // TBSteamIdInput
            // 
            TBSteamIdInput.Location = new Point(12, 87);
            TBSteamIdInput.Name = "TBSteamIdInput";
            TBSteamIdInput.Size = new Size(154, 23);
            TBSteamIdInput.TabIndex = 3;
            TBSteamIdInput.Leave += TBSteamIdInput_Leave;
            // 
            // TBFilepath
            // 
            TBFilepath.AllowDrop = true;
            TBFilepath.Location = new Point(12, 31);
            TBFilepath.Name = "TBFilepath";
            TBFilepath.Size = new Size(325, 23);
            TBFilepath.TabIndex = 1;
            TBFilepath.DragDrop += TBFilepath_DragDrop;
            TBFilepath.DragOver += TBFilepath_DragOver;
            TBFilepath.Leave += TBFilepath_Leave;
            // 
            // ButtonSelectDir
            // 
            ButtonSelectDir.AllowDrop = true;
            ButtonSelectDir.Location = new Point(343, 31);
            ButtonSelectDir.Name = "ButtonSelectDir";
            ButtonSelectDir.Size = new Size(30, 23);
            ButtonSelectDir.TabIndex = 2;
            ButtonSelectDir.Text = "📁";
            toolTip1.SetToolTip(ButtonSelectDir, "Open the Select Directory Window");
            ButtonSelectDir.UseVisualStyleBackColor = true;
            ButtonSelectDir.Click += ButtonSelectDir_Click;
            ButtonSelectDir.DragDrop += TBFilepath_DragDrop;
            ButtonSelectDir.DragOver += TBFilepath_DragOver;
            // 
            // ButtonUnpackAll
            // 
            ButtonUnpackAll.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonUnpackAll.ForeColor = SystemColors.ControlText;
            ButtonUnpackAll.Location = new Point(12, 126);
            ButtonUnpackAll.Name = "ButtonUnpackAll";
            ButtonUnpackAll.Size = new Size(84, 23);
            ButtonUnpackAll.TabIndex = 7;
            ButtonUnpackAll.Text = "UNPACK ALL";
            ButtonUnpackAll.UseVisualStyleBackColor = true;
            ButtonUnpackAll.Visible = false;
            ButtonUnpackAll.Click += ButtonUnpackAll_Click;
            // 
            // ButtonPackAll
            // 
            ButtonPackAll.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonPackAll.ForeColor = SystemColors.ControlText;
            ButtonPackAll.Location = new Point(99, 126);
            ButtonPackAll.Name = "ButtonPackAll";
            ButtonPackAll.Size = new Size(84, 23);
            ButtonPackAll.TabIndex = 8;
            ButtonPackAll.Text = "PACK ALL";
            ButtonPackAll.UseVisualStyleBackColor = true;
            ButtonPackAll.Visible = false;
            ButtonPackAll.Click += ButtonPackAll_Click;
            // 
            // pb_AppIcon
            // 
            pb_AppIcon.Image = Properties.Resources.Limebrella_Sharp_Icon_x256;
            pb_AppIcon.Location = new Point(386, 12);
            pb_AppIcon.Name = "pb_AppIcon";
            pb_AppIcon.Size = new Size(112, 117);
            pb_AppIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pb_AppIcon.TabIndex = 5;
            pb_AppIcon.TabStop = false;
            // 
            // LabelFilepath
            // 
            LabelFilepath.AutoSize = true;
            LabelFilepath.Location = new Point(12, 13);
            LabelFilepath.Name = "LabelFilepath";
            LabelFilepath.Size = new Size(98, 15);
            LabelFilepath.TabIndex = 6;
            LabelFilepath.Text = "Input Folder Path";
            // 
            // LabelSteamIdLeft1
            // 
            LabelSteamIdLeft1.AutoSize = true;
            LabelSteamIdLeft1.Location = new Point(12, 69);
            LabelSteamIdLeft1.Name = "LabelSteamIdLeft1";
            LabelSteamIdLeft1.Size = new Size(66, 15);
            LabelSteamIdLeft1.TabIndex = 7;
            LabelSteamIdLeft1.Text = "Steam32 ID";
            // 
            // LabelSteamIdRight1
            // 
            LabelSteamIdRight1.AutoSize = true;
            LabelSteamIdRight1.Location = new Point(216, 69);
            LabelSteamIdRight1.Name = "LabelSteamIdRight1";
            LabelSteamIdRight1.Size = new Size(66, 15);
            LabelSteamIdRight1.TabIndex = 9;
            LabelSteamIdRight1.Text = "Steam32 ID";
            // 
            // TBSteamIdOutput
            // 
            TBSteamIdOutput.Location = new Point(216, 87);
            TBSteamIdOutput.Name = "TBSteamIdOutput";
            TBSteamIdOutput.Size = new Size(157, 23);
            TBSteamIdOutput.TabIndex = 5;
            TBSteamIdOutput.Leave += TBSteamIdOutput_Leave;
            // 
            // LabelSteamIdLeft2
            // 
            LabelSteamIdLeft2.AutoSize = true;
            LabelSteamIdLeft2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            LabelSteamIdLeft2.Location = new Point(76, 69);
            LabelSteamIdLeft2.Name = "LabelSteamIdLeft2";
            LabelSteamIdLeft2.Size = new Size(51, 15);
            LabelSteamIdLeft2.TabIndex = 10;
            LabelSteamIdLeft2.Text = "(INPUT)";
            // 
            // LabelSteamIdRight2
            // 
            LabelSteamIdRight2.AutoSize = true;
            LabelSteamIdRight2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            LabelSteamIdRight2.Location = new Point(280, 69);
            LabelSteamIdRight2.Name = "LabelSteamIdRight2";
            LabelSteamIdRight2.Size = new Size(63, 15);
            LabelSteamIdRight2.TabIndex = 11;
            LabelSteamIdRight2.Text = "(OUTPUT)";
            // 
            // ButtonResignAll
            // 
            ButtonResignAll.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonResignAll.Location = new Point(254, 126);
            ButtonResignAll.Name = "ButtonResignAll";
            ButtonResignAll.Size = new Size(84, 23);
            ButtonResignAll.TabIndex = 9;
            ButtonResignAll.Text = "RESIGN ALL";
            ButtonResignAll.UseVisualStyleBackColor = true;
            ButtonResignAll.Click += ButtonResignAll_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripProgressBar1, toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 163);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(510, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 13;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(60, 16);
            toolStripProgressBar1.Step = 5;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(39, 17);
            toolStripStatusLabel1.Text = "Ready";
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Location = new Point(452, 130);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(46, 15);
            versionLabel.TabIndex = 14;
            versionLabel.Text = "v1.2.1.0";
            // 
            // authorLabel
            // 
            authorLabel.AutoSize = true;
            authorLabel.Cursor = Cursors.Hand;
            authorLabel.Font = new Font("Segoe UI", 7F);
            authorLabel.Location = new Point(423, 145);
            authorLabel.Name = "authorLabel";
            authorLabel.Size = new Size(75, 12);
            authorLabel.TabIndex = 15;
            authorLabel.Text = "Mi5hmasH 2024";
            authorLabel.TextAlign = ContentAlignment.MiddleRight;
            authorLabel.Click += AuthorLabel_Click;
            // 
            // ButtonInterchange
            // 
            ButtonInterchange.Location = new Point(172, 86);
            ButtonInterchange.Name = "ButtonInterchange";
            ButtonInterchange.Size = new Size(38, 23);
            ButtonInterchange.TabIndex = 4;
            ButtonInterchange.Text = "⧓";
            toolTip1.SetToolTip(ButtonInterchange, "Interchange");
            ButtonInterchange.UseVisualStyleBackColor = true;
            ButtonInterchange.Click += ButtonChangePlaces_Click;
            // 
            // ButtonBruteforceSteamId
            // 
            ButtonBruteforceSteamId.Font = new Font("Segoe UI", 8F);
            ButtonBruteforceSteamId.ForeColor = Color.Goldenrod;
            ButtonBruteforceSteamId.Location = new Point(143, 61);
            ButtonBruteforceSteamId.Name = "ButtonBruteforceSteamId";
            ButtonBruteforceSteamId.Size = new Size(23, 23);
            ButtonBruteforceSteamId.TabIndex = 6;
            ButtonBruteforceSteamId.Text = "🗝️";
            toolTip1.SetToolTip(ButtonBruteforceSteamId, "Bruteforce SteamID");
            ButtonBruteforceSteamId.UseVisualStyleBackColor = true;
            ButtonBruteforceSteamId.Visible = false;
            ButtonBruteforceSteamId.Click += ButtonBruteforceSteamId_Click;
            // 
            // ButtonOpenOutputDir
            // 
            ButtonOpenOutputDir.AllowDrop = true;
            ButtonOpenOutputDir.ForeColor = Color.Black;
            ButtonOpenOutputDir.Location = new Point(343, 126);
            ButtonOpenOutputDir.Name = "ButtonOpenOutputDir";
            ButtonOpenOutputDir.Size = new Size(30, 23);
            ButtonOpenOutputDir.TabIndex = 11;
            ButtonOpenOutputDir.Text = "📂";
            toolTip1.SetToolTip(ButtonOpenOutputDir, "Open the _OUTPUT directory");
            ButtonOpenOutputDir.UseVisualStyleBackColor = true;
            ButtonOpenOutputDir.Click += ButtonOpenOutputDir_Click;
            // 
            // ButtonAbort
            // 
            ButtonAbort.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonAbort.ForeColor = Color.Brown;
            ButtonAbort.Location = new Point(191, 126);
            ButtonAbort.Name = "ButtonAbort";
            ButtonAbort.Size = new Size(55, 23);
            ButtonAbort.TabIndex = 10;
            ButtonAbort.Text = "ABORT";
            ButtonAbort.UseVisualStyleBackColor = true;
            ButtonAbort.Visible = false;
            ButtonAbort.Click += ButtonAbort_Click;
            // 
            // superUserTimer
            // 
            superUserTimer.Interval = 500;
            superUserTimer.Tick += SuperUserTimer_Tick;
            // 
            // superUserTrigger
            // 
            superUserTrigger.BackColor = Color.Transparent;
            superUserTrigger.Location = new Point(386, 130);
            superUserTrigger.Name = "superUserTrigger";
            superUserTrigger.Size = new Size(10, 10);
            superUserTrigger.TabIndex = 16;
            superUserTrigger.TabStop = false;
            superUserTrigger.Click += SuperUserTrigger_Click;
            superUserTrigger.DoubleClick += SuperUserTrigger_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(510, 185);
            Controls.Add(superUserTrigger);
            Controls.Add(ButtonAbort);
            Controls.Add(ButtonOpenOutputDir);
            Controls.Add(ButtonBruteforceSteamId);
            Controls.Add(ButtonInterchange);
            Controls.Add(authorLabel);
            Controls.Add(versionLabel);
            Controls.Add(statusStrip1);
            Controls.Add(ButtonResignAll);
            Controls.Add(LabelSteamIdRight2);
            Controls.Add(LabelSteamIdLeft2);
            Controls.Add(LabelSteamIdRight1);
            Controls.Add(TBSteamIdOutput);
            Controls.Add(LabelSteamIdLeft1);
            Controls.Add(LabelFilepath);
            Controls.Add(pb_AppIcon);
            Controls.Add(ButtonPackAll);
            Controls.Add(ButtonUnpackAll);
            Controls.Add(ButtonSelectDir);
            Controls.Add(TBFilepath);
            Controls.Add(TBSteamIdInput);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "Limebrella Sharp";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pb_AppIcon).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)superUserTrigger).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TBSteamIdInput;
        private TextBox TBFilepath;
        private Button ButtonSelectDir;
        private Button ButtonUnpackAll;
        private Button ButtonPackAll;
        private PictureBox pb_AppIcon;
        private Label LabelFilepath;
        private Label LabelSteamIdLeft1;
        private Label LabelSteamIdRight1;
        private TextBox TBSteamIdOutput;
        private Label LabelSteamIdLeft2;
        private Label LabelSteamIdRight2;
        private Button ButtonResignAll;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Label versionLabel;
        private Label authorLabel;
        private ToolTip toolTip1;
        private Button ButtonInterchange;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button ButtonAbort;
        private Button ButtonBruteforceSteamId;
        private Button ButtonOpenOutputDir;
        private System.Windows.Forms.Timer superUserTimer;
        private PictureBox superUserTrigger;
    }
}