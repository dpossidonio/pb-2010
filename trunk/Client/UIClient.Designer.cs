namespace Client
{
    partial class UIClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.declineButton = new System.Windows.Forms.Button();
            this.acceptButton = new System.Windows.Forms.Button();
            this.friendsReqComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.sendFriendReqButton = new System.Windows.Forms.Button();
            this.serverTextBox = new System.Windows.Forms.TextBox();
            this.userTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.friendsTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.AddInterestsButton = new System.Windows.Forms.Button();
            this.InterestsComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.InterestsTextBox = new System.Windows.Forms.TextBox();
            this.UserNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.UpdateProfileButton = new System.Windows.Forms.Button();
            this.AgeComboBox = new System.Windows.Forms.ComboBox();
            this.GenderComboBox = new System.Windows.Forms.ComboBox();
            this.AgeLabel = new System.Windows.Forms.Label();
            this.GenderLabel = new System.Windows.Forms.Label();
            this.UNlabel = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.IPtextBox = new System.Windows.Forms.TextBox();
            this.Server2IPtextBox = new System.Windows.Forms.TextBox();
            this.Server3IPtextBox = new System.Windows.Forms.TextBox();
            this.Server1IPtextBox = new System.Windows.Forms.TextBox();
            this.WallTextBox = new System.Windows.Forms.TextBox();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.IPLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.RefreshViewButton = new System.Windows.Forms.Button();
            this.SendMessageButton = new System.Windows.Forms.Button();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.Msglabel = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.declineButton);
            this.tabPage3.Controls.Add(this.acceptButton);
            this.tabPage3.Controls.Add(this.friendsReqComboBox);
            this.tabPage3.Controls.Add(this.label10);
            this.tabPage3.Controls.Add(this.sendFriendReqButton);
            this.tabPage3.Controls.Add(this.serverTextBox);
            this.tabPage3.Controls.Add(this.userTextBox);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.friendsTextBox);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(642, 376);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Friends";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // declineButton
            // 
            this.declineButton.Location = new System.Drawing.Point(495, 328);
            this.declineButton.Name = "declineButton";
            this.declineButton.Size = new System.Drawing.Size(75, 23);
            this.declineButton.TabIndex = 12;
            this.declineButton.Text = "Decline";
            this.declineButton.UseVisualStyleBackColor = true;
            this.declineButton.Click += new System.EventHandler(this.declineButton_Click);
            // 
            // acceptButton
            // 
            this.acceptButton.Location = new System.Drawing.Point(381, 328);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(75, 23);
            this.acceptButton.TabIndex = 11;
            this.acceptButton.Text = "Accept";
            this.acceptButton.UseVisualStyleBackColor = true;
            this.acceptButton.Click += new System.EventHandler(this.AcceptButton_Click);
            // 
            // friendsReqComboBox
            // 
            this.friendsReqComboBox.FormattingEnabled = true;
            this.friendsReqComboBox.Location = new System.Drawing.Point(345, 266);
            this.friendsReqComboBox.Name = "friendsReqComboBox";
            this.friendsReqComboBox.Size = new System.Drawing.Size(259, 21);
            this.friendsReqComboBox.TabIndex = 10;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(434, 228);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(85, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Active Requests";
            // 
            // sendFriendReqButton
            // 
            this.sendFriendReqButton.Location = new System.Drawing.Point(419, 141);
            this.sendFriendReqButton.Name = "sendFriendReqButton";
            this.sendFriendReqButton.Size = new System.Drawing.Size(133, 23);
            this.sendFriendReqButton.TabIndex = 8;
            this.sendFriendReqButton.Text = "Send Request";
            this.sendFriendReqButton.UseVisualStyleBackColor = true;
            this.sendFriendReqButton.Click += new System.EventHandler(this.SendFriendReqButton_Click);
            // 
            // serverTextBox
            // 
            this.serverTextBox.Location = new System.Drawing.Point(419, 105);
            this.serverTextBox.Name = "serverTextBox";
            this.serverTextBox.Size = new System.Drawing.Size(185, 20);
            this.serverTextBox.TabIndex = 6;
            this.serverTextBox.Text = "127.0.0.1:";
            // 
            // userTextBox
            // 
            this.userTextBox.Location = new System.Drawing.Point(419, 79);
            this.userTextBox.Name = "userTextBox";
            this.userTextBox.Size = new System.Drawing.Size(185, 20);
            this.userTextBox.TabIndex = 5;
            this.userTextBox.Text = "xoxas";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(342, 108);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Server";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(342, 79);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Username";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(440, 55);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Friend Request";
            // 
            // friendsTextBox
            // 
            this.friendsTextBox.Location = new System.Drawing.Point(25, 55);
            this.friendsTextBox.Multiline = true;
            this.friendsTextBox.Name = "friendsTextBox";
            this.friendsTextBox.ReadOnly = true;
            this.friendsTextBox.Size = new System.Drawing.Size(250, 296);
            this.friendsTextBox.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(122, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Friends";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.AddInterestsButton);
            this.tabPage2.Controls.Add(this.InterestsComboBox);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.InterestsTextBox);
            this.tabPage2.Controls.Add(this.UserNameTextBox);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.UpdateProfileButton);
            this.tabPage2.Controls.Add(this.AgeComboBox);
            this.tabPage2.Controls.Add(this.GenderComboBox);
            this.tabPage2.Controls.Add(this.AgeLabel);
            this.tabPage2.Controls.Add(this.GenderLabel);
            this.tabPage2.Controls.Add(this.UNlabel);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(642, 376);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Profile";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // AddInterestsButton
            // 
            this.AddInterestsButton.Location = new System.Drawing.Point(314, 207);
            this.AddInterestsButton.Name = "AddInterestsButton";
            this.AddInterestsButton.Size = new System.Drawing.Size(100, 23);
            this.AddInterestsButton.TabIndex = 11;
            this.AddInterestsButton.Text = "Add Interests";
            this.AddInterestsButton.UseVisualStyleBackColor = true;
            this.AddInterestsButton.Click += new System.EventHandler(this.AddInterestsButton_Click);
            // 
            // InterestsComboBox
            // 
            this.InterestsComboBox.FormattingEnabled = true;
            this.InterestsComboBox.Location = new System.Drawing.Point(173, 207);
            this.InterestsComboBox.Name = "InterestsComboBox";
            this.InterestsComboBox.Size = new System.Drawing.Size(100, 21);
            this.InterestsComboBox.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(71, 207);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "List of Interests:";
            // 
            // InterestsTextBox
            // 
            this.InterestsTextBox.Location = new System.Drawing.Point(161, 146);
            this.InterestsTextBox.Name = "InterestsTextBox";
            this.InterestsTextBox.ReadOnly = true;
            this.InterestsTextBox.Size = new System.Drawing.Size(356, 20);
            this.InterestsTextBox.TabIndex = 8;
            // 
            // UserNameTextBox
            // 
            this.UserNameTextBox.Location = new System.Drawing.Point(161, 40);
            this.UserNameTextBox.Name = "UserNameTextBox";
            this.UserNameTextBox.Size = new System.Drawing.Size(100, 20);
            this.UserNameTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(71, 153);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Interests:";
            // 
            // UpdateProfileButton
            // 
            this.UpdateProfileButton.Location = new System.Drawing.Point(261, 274);
            this.UpdateProfileButton.Name = "UpdateProfileButton";
            this.UpdateProfileButton.Size = new System.Drawing.Size(100, 23);
            this.UpdateProfileButton.TabIndex = 6;
            this.UpdateProfileButton.Text = "Update";
            this.UpdateProfileButton.UseVisualStyleBackColor = true;
            this.UpdateProfileButton.Click += new System.EventHandler(this.UpdateProfileButton_Click);
            // 
            // AgeComboBox
            // 
            this.AgeComboBox.FormattingEnabled = true;
            this.AgeComboBox.Location = new System.Drawing.Point(357, 96);
            this.AgeComboBox.Name = "AgeComboBox";
            this.AgeComboBox.Size = new System.Drawing.Size(82, 21);
            this.AgeComboBox.TabIndex = 5;
            // 
            // GenderComboBox
            // 
            this.GenderComboBox.FormattingEnabled = true;
            this.GenderComboBox.Location = new System.Drawing.Point(161, 93);
            this.GenderComboBox.Name = "GenderComboBox";
            this.GenderComboBox.Size = new System.Drawing.Size(82, 21);
            this.GenderComboBox.TabIndex = 4;
            // 
            // AgeLabel
            // 
            this.AgeLabel.AutoSize = true;
            this.AgeLabel.Location = new System.Drawing.Point(311, 96);
            this.AgeLabel.Name = "AgeLabel";
            this.AgeLabel.Size = new System.Drawing.Size(29, 13);
            this.AgeLabel.TabIndex = 3;
            this.AgeLabel.Text = "Age:";
            // 
            // GenderLabel
            // 
            this.GenderLabel.AutoSize = true;
            this.GenderLabel.Location = new System.Drawing.Point(76, 96);
            this.GenderLabel.Name = "GenderLabel";
            this.GenderLabel.Size = new System.Drawing.Size(45, 13);
            this.GenderLabel.TabIndex = 2;
            this.GenderLabel.Text = "Gender:";
            // 
            // UNlabel
            // 
            this.UNlabel.AutoSize = true;
            this.UNlabel.Location = new System.Drawing.Point(76, 47);
            this.UNlabel.Name = "UNlabel";
            this.UNlabel.Size = new System.Drawing.Size(58, 13);
            this.UNlabel.TabIndex = 0;
            this.UNlabel.Text = "Username:";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.IPtextBox);
            this.tabPage1.Controls.Add(this.Server2IPtextBox);
            this.tabPage1.Controls.Add(this.Server3IPtextBox);
            this.tabPage1.Controls.Add(this.Server1IPtextBox);
            this.tabPage1.Controls.Add(this.WallTextBox);
            this.tabPage1.Controls.Add(this.MessageTextBox);
            this.tabPage1.Controls.Add(this.IPLabel);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.RefreshViewButton);
            this.tabPage1.Controls.Add(this.SendMessageButton);
            this.tabPage1.Controls.Add(this.ServerLabel);
            this.tabPage1.Controls.Add(this.ConnectButton);
            this.tabPage1.Controls.Add(this.Msglabel);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(642, 376);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Home";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // IPtextBox
            // 
            this.IPtextBox.Location = new System.Drawing.Point(78, 13);
            this.IPtextBox.Name = "IPtextBox";
            this.IPtextBox.Size = new System.Drawing.Size(85, 20);
            this.IPtextBox.TabIndex = 16;
            this.IPtextBox.Text = "127.0.0.1:8000";
            // 
            // Server2IPtextBox
            // 
            this.Server2IPtextBox.Location = new System.Drawing.Point(401, 12);
            this.Server2IPtextBox.Name = "Server2IPtextBox";
            this.Server2IPtextBox.Size = new System.Drawing.Size(104, 20);
            this.Server2IPtextBox.TabIndex = 11;
            // 
            // Server3IPtextBox
            // 
            this.Server3IPtextBox.Location = new System.Drawing.Point(535, 13);
            this.Server3IPtextBox.Name = "Server3IPtextBox";
            this.Server3IPtextBox.Size = new System.Drawing.Size(104, 20);
            this.Server3IPtextBox.TabIndex = 10;
            // 
            // Server1IPtextBox
            // 
            this.Server1IPtextBox.Location = new System.Drawing.Point(265, 12);
            this.Server1IPtextBox.Name = "Server1IPtextBox";
            this.Server1IPtextBox.Size = new System.Drawing.Size(104, 20);
            this.Server1IPtextBox.TabIndex = 7;
            this.Server1IPtextBox.Text = "127.0.0.1:8001";
            // 
            // WallTextBox
            // 
            this.WallTextBox.Location = new System.Drawing.Point(20, 101);
            this.WallTextBox.Multiline = true;
            this.WallTextBox.Name = "WallTextBox";
            this.WallTextBox.ReadOnly = true;
            this.WallTextBox.Size = new System.Drawing.Size(610, 264);
            this.WallTextBox.TabIndex = 4;
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.Location = new System.Drawing.Point(124, 70);
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.Size = new System.Drawing.Size(366, 20);
            this.MessageTextBox.TabIndex = 3;
            // 
            // IPLabel
            // 
            this.IPLabel.AutoSize = true;
            this.IPLabel.Location = new System.Drawing.Point(27, 16);
            this.IPLabel.Name = "IPLabel";
            this.IPLabel.Size = new System.Drawing.Size(45, 13);
            this.IPLabel.TabIndex = 15;
            this.IPLabel.Text = "[IP:Port]";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(375, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "#2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(509, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "#3";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(239, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "#1";
            // 
            // RefreshViewButton
            // 
            this.RefreshViewButton.Location = new System.Drawing.Point(564, 68);
            this.RefreshViewButton.Name = "RefreshViewButton";
            this.RefreshViewButton.Size = new System.Drawing.Size(62, 23);
            this.RefreshViewButton.TabIndex = 9;
            this.RefreshViewButton.Text = "Refresh";
            this.RefreshViewButton.UseVisualStyleBackColor = true;
            this.RefreshViewButton.Click += new System.EventHandler(this.RefreshViewButton_Click);
            // 
            // SendMessageButton
            // 
            this.SendMessageButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.SendMessageButton.Location = new System.Drawing.Point(496, 68);
            this.SendMessageButton.Name = "SendMessageButton";
            this.SendMessageButton.Size = new System.Drawing.Size(62, 23);
            this.SendMessageButton.TabIndex = 8;
            this.SendMessageButton.Text = "Say";
            this.SendMessageButton.UseVisualStyleBackColor = true;
            this.SendMessageButton.Click += new System.EventHandler(this.SendMessageButton_Click);
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Location = new System.Drawing.Point(187, 15);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(46, 13);
            this.ServerLabel.TabIndex = 6;
            this.ServerLabel.Text = "Servers:";
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(190, 38);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 5;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // Msglabel
            // 
            this.Msglabel.AutoSize = true;
            this.Msglabel.Location = new System.Drawing.Point(17, 73);
            this.Msglabel.Name = "Msglabel";
            this.Msglabel.Size = new System.Drawing.Size(94, 13);
            this.Msglabel.TabIndex = 2;
            this.Msglabel.Text = "What do you feel?";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(650, 402);
            this.tabControl.TabIndex = 0;
            // 
            // UIClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 426);
            this.Controls.Add(this.tabControl);
            this.Name = "UIClient";
            this.Text = "PADIbook - Client";
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button AddInterestsButton;
        private System.Windows.Forms.ComboBox InterestsComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox InterestsTextBox;
        private System.Windows.Forms.TextBox UserNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button UpdateProfileButton;
        private System.Windows.Forms.ComboBox AgeComboBox;
        private System.Windows.Forms.ComboBox GenderComboBox;
        private System.Windows.Forms.Label AgeLabel;
        private System.Windows.Forms.Label GenderLabel;
        private System.Windows.Forms.Label UNlabel;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox IPtextBox;
        private System.Windows.Forms.TextBox Server2IPtextBox;
        private System.Windows.Forms.TextBox Server3IPtextBox;
        private System.Windows.Forms.TextBox Server1IPtextBox;
        private System.Windows.Forms.TextBox WallTextBox;
        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.Label IPLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button RefreshViewButton;
        private System.Windows.Forms.Button SendMessageButton;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Label Msglabel;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TextBox friendsTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button sendFriendReqButton;
        private System.Windows.Forms.TextBox serverTextBox;
        private System.Windows.Forms.TextBox userTextBox;
        private System.Windows.Forms.Button declineButton;
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.ComboBox friendsReqComboBox;



    }
}

