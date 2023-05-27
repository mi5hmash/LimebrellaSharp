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
            TBSteamIdLeft = new TextBox();
            TBFilepath = new TextBox();
            ButtonSelectDir = new Button();
            ButtonUnpackAll = new Button();
            ButtonPackAll = new Button();
            pictureBox1 = new PictureBox();
            LabelFilepath = new Label();
            LabelSteamIdLeft1 = new Label();
            LabelSteamIdRight1 = new Label();
            TBSteamIdRight = new TextBox();
            LabelSteamIdLeft2 = new Label();
            LabelSteamIdRight2 = new Label();
            ButtonResignAll = new Button();
            statusStrip1 = new StatusStrip();
            toolStripProgressBar1 = new ToolStripProgressBar();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            label6 = new Label();
            label7 = new Label();
            toolTip1 = new ToolTip(components);
            ButtonChangePlaces = new Button();
            ButtonBruteforceSteamID = new Button();
            ButtonOpenOutputDir = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            ButtonAbort = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // TBSteamIdLeft
            // 
            TBSteamIdLeft.Location = new Point(12, 87);
            TBSteamIdLeft.Name = "TBSteamIdLeft";
            TBSteamIdLeft.Size = new Size(154, 23);
            TBSteamIdLeft.TabIndex = 3;
            // 
            // TBFilepath
            // 
            TBFilepath.AllowDrop = true;
            TBFilepath.Location = new Point(12, 31);
            TBFilepath.Name = "TBFilepath";
            TBFilepath.Size = new Size(315, 23);
            TBFilepath.TabIndex = 1;
            TBFilepath.TextChanged += TBFilepath_TextChanged;
            TBFilepath.DragDrop += TBFilepath_DragDrop;
            TBFilepath.DragOver += TBFilepath_DragOver;
            // 
            // ButtonSelectDir
            // 
            ButtonSelectDir.AllowDrop = true;
            ButtonSelectDir.Location = new Point(333, 31);
            ButtonSelectDir.Name = "ButtonSelectDir";
            ButtonSelectDir.Size = new Size(37, 23);
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
            ButtonUnpackAll.Location = new Point(12, 126);
            ButtonUnpackAll.Name = "ButtonUnpackAll";
            ButtonUnpackAll.Size = new Size(75, 23);
            ButtonUnpackAll.TabIndex = 6;
            ButtonUnpackAll.Text = "Unpack All";
            ButtonUnpackAll.UseVisualStyleBackColor = true;
            ButtonUnpackAll.Click += ButtonUnpackAll_Click;
            // 
            // ButtonPackAll
            // 
            ButtonPackAll.Location = new Point(174, 126);
            ButtonPackAll.Name = "ButtonPackAll";
            ButtonPackAll.Size = new Size(75, 23);
            ButtonPackAll.TabIndex = 8;
            ButtonPackAll.Text = "Pack All";
            ButtonPackAll.UseVisualStyleBackColor = true;
            ButtonPackAll.Click += ButtonPackAll_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.Limebrella_Sharp_Icon_x512;
            pictureBox1.Location = new Point(386, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(112, 117);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
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
            // TBSteamIdRight
            // 
            TBSteamIdRight.Location = new Point(216, 87);
            TBSteamIdRight.Name = "TBSteamIdRight";
            TBSteamIdRight.Size = new Size(154, 23);
            TBSteamIdRight.TabIndex = 5;
            // 
            // LabelSteamIdLeft2
            // 
            LabelSteamIdLeft2.AutoSize = true;
            LabelSteamIdLeft2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            LabelSteamIdLeft2.Location = new Point(76, 69);
            LabelSteamIdLeft2.Name = "LabelSteamIdLeft2";
            LabelSteamIdLeft2.Size = new Size(51, 15);
            LabelSteamIdLeft2.TabIndex = 10;
            LabelSteamIdLeft2.Text = "(INPUT)";
            // 
            // LabelSteamIdRight2
            // 
            LabelSteamIdRight2.AutoSize = true;
            LabelSteamIdRight2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            LabelSteamIdRight2.Location = new Point(280, 69);
            LabelSteamIdRight2.Name = "LabelSteamIdRight2";
            LabelSteamIdRight2.Size = new Size(63, 15);
            LabelSteamIdRight2.TabIndex = 11;
            LabelSteamIdRight2.Text = "(OUTPUT)";
            // 
            // ButtonResignAll
            // 
            ButtonResignAll.Location = new Point(93, 126);
            ButtonResignAll.Name = "ButtonResignAll";
            ButtonResignAll.Size = new Size(75, 23);
            ButtonResignAll.TabIndex = 7;
            ButtonResignAll.Text = "Resign All";
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
            toolStripProgressBar1.Size = new Size(70, 16);
            toolStripProgressBar1.Step = 5;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(39, 17);
            toolStripStatusLabel1.Text = "Ready";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(461, 130);
            label6.Name = "label6";
            label6.Size = new Size(37, 15);
            label6.TabIndex = 14;
            label6.Text = "v1.0.0";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point);
            label7.Location = new Point(423, 145);
            label7.Name = "label7";
            label7.Size = new Size(75, 12);
            label7.TabIndex = 15;
            label7.Text = "Mi5hmasH 2023";
            // 
            // ButtonChangePlaces
            // 
            ButtonChangePlaces.Location = new Point(172, 86);
            ButtonChangePlaces.Name = "ButtonChangePlaces";
            ButtonChangePlaces.Size = new Size(38, 23);
            ButtonChangePlaces.TabIndex = 4;
            ButtonChangePlaces.Text = "⧓";
            toolTip1.SetToolTip(ButtonChangePlaces, "Change places");
            ButtonChangePlaces.UseVisualStyleBackColor = true;
            ButtonChangePlaces.Click += ButtonChangePlaces_Click;
            // 
            // ButtonBruteforceSteamID
            // 
            ButtonBruteforceSteamID.Enabled = false;
            ButtonBruteforceSteamID.Location = new Point(144, 61);
            ButtonBruteforceSteamID.Name = "ButtonBruteforceSteamID";
            ButtonBruteforceSteamID.Size = new Size(22, 23);
            ButtonBruteforceSteamID.TabIndex = 10;
            ButtonBruteforceSteamID.Text = "🔍";
            toolTip1.SetToolTip(ButtonBruteforceSteamID, "Bruteforce SteamID");
            ButtonBruteforceSteamID.UseVisualStyleBackColor = true;
            ButtonBruteforceSteamID.Visible = false;
            ButtonBruteforceSteamID.Click += ButtonBruteforceSteamID_Click;
            // 
            // ButtonOpenOutputDir
            // 
            ButtonOpenOutputDir.AllowDrop = true;
            ButtonOpenOutputDir.ForeColor = Color.OliveDrab;
            ButtonOpenOutputDir.Location = new Point(254, 126);
            ButtonOpenOutputDir.Name = "ButtonOpenOutputDir";
            ButtonOpenOutputDir.Size = new Size(37, 23);
            ButtonOpenOutputDir.TabIndex = 10;
            ButtonOpenOutputDir.Text = "📁";
            toolTip1.SetToolTip(ButtonOpenOutputDir, "Open the _OUTPUT directory");
            ButtonOpenOutputDir.UseVisualStyleBackColor = true;
            ButtonOpenOutputDir.Click += ButtonOpenOutputDir_Click;
            // 
            // ButtonAbort
            // 
            ButtonAbort.ForeColor = Color.Brown;
            ButtonAbort.Location = new Point(295, 126);
            ButtonAbort.Name = "ButtonAbort";
            ButtonAbort.Size = new Size(75, 23);
            ButtonAbort.TabIndex = 9;
            ButtonAbort.Text = "Abort";
            ButtonAbort.UseVisualStyleBackColor = true;
            ButtonAbort.Visible = false;
            ButtonAbort.Click += ButtonAbort_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(510, 185);
            Controls.Add(ButtonOpenOutputDir);
            Controls.Add(ButtonBruteforceSteamID);
            Controls.Add(ButtonAbort);
            Controls.Add(ButtonChangePlaces);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(statusStrip1);
            Controls.Add(ButtonResignAll);
            Controls.Add(LabelSteamIdRight2);
            Controls.Add(LabelSteamIdLeft2);
            Controls.Add(LabelSteamIdRight1);
            Controls.Add(TBSteamIdRight);
            Controls.Add(LabelSteamIdLeft1);
            Controls.Add(LabelFilepath);
            Controls.Add(pictureBox1);
            Controls.Add(ButtonPackAll);
            Controls.Add(ButtonUnpackAll);
            Controls.Add(ButtonSelectDir);
            Controls.Add(TBFilepath);
            Controls.Add(TBSteamIdLeft);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "Limebrella Sharp";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TBSteamIdLeft;
        private TextBox TBFilepath;
        private Button ButtonSelectDir;
        private Button ButtonUnpackAll;
        private Button ButtonPackAll;
        private PictureBox pictureBox1;
        private Label LabelFilepath;
        private Label LabelSteamIdLeft1;
        private Label LabelSteamIdRight1;
        private TextBox TBSteamIdRight;
        private Label LabelSteamIdLeft2;
        private Label LabelSteamIdRight2;
        private Button ButtonResignAll;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Label label6;
        private Label label7;
        private ToolTip toolTip1;
        private Button ButtonChangePlaces;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button ButtonAbort;
        private Button ButtonBruteforceSteamID;
        private Button ButtonOpenOutputDir;
    }
}