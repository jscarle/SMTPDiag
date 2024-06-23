using System.ComponentModel;

namespace SMTPDiag3.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            hostLabel = new Label();
            hostTextBox = new TextBox();
            portLabel = new Label();
            portTextBox = new TextBox();
            usernameLabel = new Label();
            usernameTextBox = new TextBox();
            passwordLabel = new Label();
            passwordTextBox = new TextBox();
            authenticationLabel = new Label();
            authenticationComboBox = new ComboBox();
            fromAddressLabel = new Label();
            fromAddressTextBox = new TextBox();
            toAddressLabel = new Label();
            toAddressTextBox = new TextBox();
            sendButton = new Button();
            inputPanel = new Panel();
            resultsTextBox = new TextBox();
            inputPanel.SuspendLayout();
            SuspendLayout();
            // 
            // hostLabel
            // 
            hostLabel.AutoSize = true;
            hostLabel.Location = new Point(12, 9);
            hostLabel.Name = "hostLabel";
            hostLabel.Size = new Size(35, 17);
            hostLabel.TabIndex = 0;
            hostLabel.Text = "Host";
            // 
            // hostTextBox
            // 
            hostTextBox.Location = new Point(15, 30);
            hostTextBox.Margin = new Padding(3, 4, 3, 4);
            hostTextBox.Name = "hostTextBox";
            hostTextBox.Size = new Size(250, 25);
            hostTextBox.TabIndex = 1;
            // 
            // portLabel
            // 
            portLabel.AutoSize = true;
            portLabel.Location = new Point(268, 9);
            portLabel.Name = "portLabel";
            portLabel.Size = new Size(32, 17);
            portLabel.TabIndex = 2;
            portLabel.Text = "Port";
            // 
            // portTextBox
            // 
            portTextBox.Location = new Point(271, 30);
            portTextBox.Margin = new Padding(3, 4, 3, 4);
            portTextBox.Name = "portTextBox";
            portTextBox.Size = new Size(44, 25);
            portTextBox.TabIndex = 3;
            portTextBox.Text = "25";
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.Location = new Point(318, 9);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(67, 17);
            usernameLabel.TabIndex = 4;
            usernameLabel.Text = "Username";
            // 
            // usernameTextBox
            // 
            usernameTextBox.Location = new Point(321, 30);
            usernameTextBox.Margin = new Padding(3, 4, 3, 4);
            usernameTextBox.Name = "usernameTextBox";
            usernameTextBox.Size = new Size(187, 25);
            usernameTextBox.TabIndex = 5;
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Location = new Point(511, 9);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(64, 17);
            passwordLabel.TabIndex = 6;
            passwordLabel.Text = "Password";
            // 
            // passwordTextBox
            // 
            passwordTextBox.Location = new Point(514, 30);
            passwordTextBox.Margin = new Padding(3, 4, 3, 4);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.Size = new Size(187, 25);
            passwordTextBox.TabIndex = 7;
            // 
            // authenticationLabel
            // 
            authenticationLabel.AutoSize = true;
            authenticationLabel.Location = new Point(704, 9);
            authenticationLabel.Name = "authenticationLabel";
            authenticationLabel.Size = new Size(90, 17);
            authenticationLabel.TabIndex = 8;
            authenticationLabel.Text = "Authentication";
            // 
            // authenticationComboBox
            // 
            authenticationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            authenticationComboBox.FormattingEnabled = true;
            authenticationComboBox.Items.AddRange(new object[] { "None", "PLAIN", "LOGIN", "CRAM-MD5", "Force PLAIN", "Force LOGIN", "Force CRAM-MD5" });
            authenticationComboBox.Location = new Point(707, 30);
            authenticationComboBox.Name = "authenticationComboBox";
            authenticationComboBox.Size = new Size(140, 25);
            authenticationComboBox.TabIndex = 9;
            // 
            // fromAddressLabel
            // 
            fromAddressLabel.AutoSize = true;
            fromAddressLabel.Location = new Point(12, 59);
            fromAddressLabel.Name = "fromAddressLabel";
            fromAddressLabel.Size = new Size(90, 17);
            fromAddressLabel.TabIndex = 10;
            fromAddressLabel.Text = "From Address";
            // 
            // fromAddressTextBox
            // 
            fromAddressTextBox.Location = new Point(15, 80);
            fromAddressTextBox.Margin = new Padding(3, 4, 3, 4);
            fromAddressTextBox.Name = "fromAddressTextBox";
            fromAddressTextBox.Size = new Size(300, 25);
            fromAddressTextBox.TabIndex = 11;
            // 
            // toAddressTextBox
            // 
            toAddressTextBox.Location = new Point(321, 80);
            toAddressTextBox.Margin = new Padding(3, 4, 3, 4);
            toAddressTextBox.Name = "toAddressTextBox";
            toAddressTextBox.Size = new Size(300, 25);
            toAddressTextBox.TabIndex = 13;
            // 
            // toAddressLabel
            // 
            toAddressLabel.AutoSize = true;
            toAddressLabel.Location = new Point(318, 59);
            toAddressLabel.Name = "toAddressLabel";
            toAddressLabel.Size = new Size(74, 17);
            toAddressLabel.TabIndex = 12;
            toAddressLabel.Text = "To Address";
            // 
            // sendButton
            // 
            sendButton.Location = new Point(627, 60);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(220, 45);
            sendButton.TabIndex = 14;
            sendButton.Text = "Send";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += SendButton_Click;
            // 
            // inputPanel
            // 
            inputPanel.Controls.Add(authenticationLabel);
            inputPanel.Controls.Add(authenticationComboBox);
            inputPanel.Controls.Add(hostLabel);
            inputPanel.Controls.Add(hostTextBox);
            inputPanel.Controls.Add(sendButton);
            inputPanel.Controls.Add(fromAddressLabel);
            inputPanel.Controls.Add(toAddressTextBox);
            inputPanel.Controls.Add(fromAddressTextBox);
            inputPanel.Controls.Add(toAddressLabel);
            inputPanel.Controls.Add(portLabel);
            inputPanel.Controls.Add(passwordTextBox);
            inputPanel.Controls.Add(portTextBox);
            inputPanel.Controls.Add(passwordLabel);
            inputPanel.Controls.Add(usernameLabel);
            inputPanel.Controls.Add(usernameTextBox);
            inputPanel.Dock = DockStyle.Top;
            inputPanel.Location = new Point(0, 0);
            inputPanel.Name = "inputPanel";
            inputPanel.Size = new Size(864, 122);
            inputPanel.TabIndex = 14;
            // 
            // resultsTextBox
            // 
            resultsTextBox.BackColor = SystemColors.Info;
            resultsTextBox.Dock = DockStyle.Fill;
            resultsTextBox.Font = new Font("Courier New", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            resultsTextBox.ForeColor = SystemColors.InfoText;
            resultsTextBox.Location = new Point(0, 122);
            resultsTextBox.Multiline = true;
            resultsTextBox.Name = "resultsTextBox";
            resultsTextBox.ReadOnly = true;
            resultsTextBox.ScrollBars = ScrollBars.Both;
            resultsTextBox.Size = new Size(864, 339);
            resultsTextBox.TabIndex = 13;
            resultsTextBox.WordWrap = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(864, 461);
            Controls.Add(resultsTextBox);
            Controls.Add(inputPanel);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(880, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SMTP Diagnostics";
            Load += MainForm_Load;
            inputPanel.ResumeLayout(false);
            inputPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label hostLabel;
        private TextBox hostTextBox;
        private Label portLabel;
        private TextBox portTextBox;
        private Label usernameLabel;
        private TextBox usernameTextBox;
        private Label passwordLabel;
        private TextBox passwordTextBox;
        private Label authenticationLabel;
        private ComboBox authenticationComboBox;
        private Label fromAddressLabel;
        private TextBox fromAddressTextBox;
        private Label toAddressLabel;
        private TextBox toAddressTextBox;
        private Button sendButton;
        private Panel inputPanel;
        private TextBox resultsTextBox;
    }
}

