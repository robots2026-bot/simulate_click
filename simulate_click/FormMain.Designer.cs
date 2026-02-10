namespace simulate_click
{
    partial class FormMain
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
            layoutMain = new TableLayoutPanel();
            panelTop = new FlowLayoutPanel();
            lblHost = new Label();
            txtHost = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            lblUser = new Label();
            txtUser = new TextBox();
            lblPass = new Label();
            txtPass = new TextBox();
            lblRefresh = new Label();
            cboRefresh = new ComboBox();
            chkRgba = new CheckBox();
            btnConnect = new Button();
            btnRefresh = new Button();
            picScreen = new PictureBox();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusLabelClick = new ToolStripStatusLabel();
            layoutMain.SuspendLayout();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picScreen).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // layoutMain
            // 
            layoutMain.ColumnCount = 1;
            layoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutMain.Dock = DockStyle.Fill;
            layoutMain.RowCount = 3;
            layoutMain.RowStyles.Add(new RowStyle());
            layoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutMain.RowStyles.Add(new RowStyle());
            layoutMain.Controls.Add(panelTop, 0, 0);
            layoutMain.Controls.Add(picScreen, 0, 1);
            layoutMain.Controls.Add(statusStrip, 0, 2);
            // 
            // panelTop
            // 
            panelTop.AutoSize = true;
            panelTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelTop.Dock = DockStyle.Fill;
            panelTop.Padding = new Padding(8, 8, 8, 4);
            panelTop.WrapContents = true;
            panelTop.Controls.Add(lblHost);
            panelTop.Controls.Add(txtHost);
            panelTop.Controls.Add(lblPort);
            panelTop.Controls.Add(txtPort);
            panelTop.Controls.Add(lblUser);
            panelTop.Controls.Add(txtUser);
            panelTop.Controls.Add(lblPass);
            panelTop.Controls.Add(txtPass);
            panelTop.Controls.Add(lblRefresh);
            panelTop.Controls.Add(cboRefresh);
            panelTop.Controls.Add(chkRgba);
            panelTop.Controls.Add(btnConnect);
            panelTop.Controls.Add(btnRefresh);
            // 
            // lblHost
            // 
            lblHost.AutoSize = true;
            lblHost.Margin = new Padding(0, 8, 4, 0);
            lblHost.Text = "Host";
            // 
            // txtHost
            // 
            txtHost.Margin = new Padding(0, 4, 12, 0);
            txtHost.Size = new Size(140, 23);
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Margin = new Padding(0, 8, 4, 0);
            lblPort.Text = "Port";
            // 
            // txtPort
            // 
            txtPort.Margin = new Padding(0, 4, 12, 0);
            txtPort.Size = new Size(60, 23);
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Margin = new Padding(0, 8, 4, 0);
            lblUser.Text = "User";
            // 
            // txtUser
            // 
            txtUser.Margin = new Padding(0, 4, 12, 0);
            txtUser.Size = new Size(90, 23);
            // 
            // lblPass
            // 
            lblPass.AutoSize = true;
            lblPass.Margin = new Padding(0, 8, 4, 0);
            lblPass.Text = "Pass";
            // 
            // txtPass
            // 
            txtPass.Margin = new Padding(0, 4, 12, 0);
            txtPass.PasswordChar = '*';
            txtPass.Size = new Size(100, 23);
            // 
            // lblRefresh
            // 
            lblRefresh.AutoSize = true;
            lblRefresh.Margin = new Padding(0, 8, 4, 0);
            lblRefresh.Text = "Refresh(s)";
            // 
            // cboRefresh
            // 
            cboRefresh.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRefresh.Margin = new Padding(0, 4, 12, 0);
            cboRefresh.Size = new Size(80, 23);
            // 
            // chkRgba
            // 
            chkRgba.AutoSize = true;
            chkRgba.Margin = new Padding(0, 8, 12, 0);
            chkRgba.Text = "Use RGBA";
            // 
            // btnConnect
            // 
            btnConnect.AutoSize = true;
            btnConnect.Margin = new Padding(0, 4, 8, 0);
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            btnRefresh.AutoSize = true;
            btnRefresh.Margin = new Padding(0, 4, 0, 0);
            btnRefresh.Text = "Refresh Now";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // picScreen
            // 
            picScreen.BackColor = Color.Black;
            picScreen.Dock = DockStyle.Fill;
            picScreen.SizeMode = PictureBoxSizeMode.Zoom;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, statusLabelClick });
            statusStrip.SizingGrip = false;
            // 
            // statusLabel
            // 
            statusLabel.Text = "Idle";
            // 
            // statusLabelClick
            // 
            statusLabelClick.Text = "Click: -";
            // 
            // FormMain
            // 
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            Controls.Add(layoutMain);
            Text = "Remote Touch Viewer";
            layoutMain.ResumeLayout(false);
            layoutMain.PerformLayout();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picScreen).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel layoutMain;
        private FlowLayoutPanel panelTop;
        private Label lblHost;
        private TextBox txtHost;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblUser;
        private TextBox txtUser;
        private Label lblPass;
        private TextBox txtPass;
        private Label lblRefresh;
        private ComboBox cboRefresh;
        private CheckBox chkRgba;
        private Button btnConnect;
        private Button btnRefresh;
        private PictureBox picScreen;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel statusLabelClick;
    }
}
