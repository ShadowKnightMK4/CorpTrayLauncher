namespace GroupEditor
{
    partial class GroupEditorForm
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
            this.components = new System.ComponentModel.Container();
            this.ToolTipSettingHelpShow = new System.Windows.Forms.ToolTip(this.components);
            this.TabPageGroupEditorTab = new System.Windows.Forms.TabPage();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.GroupBoxNames = new System.Windows.Forms.GroupBox();
            this.TextBoxGroupBoxName = new System.Windows.Forms.TextBox();
            this.LabelGroupBoxNameText = new System.Windows.Forms.Label();
            this.GroupBoxFolderLocations = new System.Windows.Forms.GroupBox();
            this.ButtonClickToAdd = new System.Windows.Forms.Button();
            this.ButtonSelectAllCheckedFolderList = new System.Windows.Forms.Button();
            this.ButtonClearCheckedFolderList = new System.Windows.Forms.Button();
            this.ButtonBrowseForNewFolder = new System.Windows.Forms.Button();
            this.TextBoxFreeFormFolder = new System.Windows.Forms.TextBox();
            this.checkedListBoxFolderSelection = new System.Windows.Forms.CheckedListBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.CheckBoxIsTopLevelGroup = new System.Windows.Forms.CheckBox();
            this.CheckBoxIsDisabled = new System.Windows.Forms.CheckBox();
            this.GroupIconSetting = new System.Windows.Forms.GroupBox();
            this.CheckBoxNoGroupIcon = new System.Windows.Forms.CheckBox();
            this.ButtonBrowseForGroupIcon = new System.Windows.Forms.Button();
            this.TextBoxGroupIconSource = new System.Windows.Forms.TextBox();
            this.LabeGrouplconSource = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TabPagePolicyLevelSettings = new System.Windows.Forms.TabPage();
            this.TabPageTraySettings = new System.Windows.Forms.TabPage();
            this.FolderBrowserDialogSelectLinkFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.ToolTipShowFolderLocation = new System.Windows.Forms.ToolTip(this.components);
            this.ListBoxGroupList = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.TabPageGroupEditorTab.SuspendLayout();
            this.GroupBoxNames.SuspendLayout();
            this.GroupBoxFolderLocations.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.GroupIconSetting.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabPageGroupEditorTab
            // 
            this.TabPageGroupEditorTab.Controls.Add(this.button7);
            this.TabPageGroupEditorTab.Controls.Add(this.button6);
            this.TabPageGroupEditorTab.Controls.Add(this.button5);
            this.TabPageGroupEditorTab.Controls.Add(this.GroupBoxNames);
            this.TabPageGroupEditorTab.Controls.Add(this.GroupBoxFolderLocations);
            this.TabPageGroupEditorTab.Controls.Add(this.groupBox5);
            this.TabPageGroupEditorTab.Controls.Add(this.GroupIconSetting);
            this.TabPageGroupEditorTab.Location = new System.Drawing.Point(4, 29);
            this.TabPageGroupEditorTab.Name = "TabPageGroupEditorTab";
            this.TabPageGroupEditorTab.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageGroupEditorTab.Size = new System.Drawing.Size(743, 487);
            this.TabPageGroupEditorTab.TabIndex = 1;
            this.TabPageGroupEditorTab.Text = "Group Editor";
            this.TabPageGroupEditorTab.UseVisualStyleBackColor = true;
            this.TabPageGroupEditorTab.Click += new System.EventHandler(this.tabPageUserSetting_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(17, 429);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(150, 32);
            this.button7.TabIndex = 16;
            this.button7.Text = "Export REG File";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(323, 429);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(179, 32);
            this.button6.TabIndex = 15;
            this.button6.Text = "Save As User Group";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(509, 429);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(179, 32);
            this.button5.TabIndex = 14;
            this.button5.Text = "Save As Policy Group";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // GroupBoxNames
            // 
            this.GroupBoxNames.Controls.Add(this.TextBoxGroupBoxName);
            this.GroupBoxNames.Controls.Add(this.LabelGroupBoxNameText);
            this.GroupBoxNames.Location = new System.Drawing.Point(367, 24);
            this.GroupBoxNames.Name = "GroupBoxNames";
            this.GroupBoxNames.Size = new System.Drawing.Size(360, 111);
            this.GroupBoxNames.TabIndex = 13;
            this.GroupBoxNames.TabStop = false;
            this.GroupBoxNames.Text = "Group Name";
            // 
            // TextBoxGroupBoxName
            // 
            this.TextBoxGroupBoxName.Location = new System.Drawing.Point(6, 51);
            this.TextBoxGroupBoxName.Name = "TextBoxGroupBoxName";
            this.TextBoxGroupBoxName.Size = new System.Drawing.Size(250, 26);
            this.TextBoxGroupBoxName.TabIndex = 8;
            this.TextBoxGroupBoxName.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxGroupBoxName_Validating);
            // 
            // LabelGroupBoxNameText
            // 
            this.LabelGroupBoxNameText.AutoSize = true;
            this.LabelGroupBoxNameText.Location = new System.Drawing.Point(6, 22);
            this.LabelGroupBoxNameText.Name = "LabelGroupBoxNameText";
            this.LabelGroupBoxNameText.Size = new System.Drawing.Size(185, 20);
            this.LabelGroupBoxNameText.TabIndex = 9;
            this.LabelGroupBoxNameText.Text = "Type Group Name Below";
            // 
            // GroupBoxFolderLocations
            // 
            this.GroupBoxFolderLocations.Controls.Add(this.ButtonClickToAdd);
            this.GroupBoxFolderLocations.Controls.Add(this.ButtonSelectAllCheckedFolderList);
            this.GroupBoxFolderLocations.Controls.Add(this.ButtonClearCheckedFolderList);
            this.GroupBoxFolderLocations.Controls.Add(this.ButtonBrowseForNewFolder);
            this.GroupBoxFolderLocations.Controls.Add(this.TextBoxFreeFormFolder);
            this.GroupBoxFolderLocations.Controls.Add(this.checkedListBoxFolderSelection);
            this.GroupBoxFolderLocations.Location = new System.Drawing.Point(367, 141);
            this.GroupBoxFolderLocations.Name = "GroupBoxFolderLocations";
            this.GroupBoxFolderLocations.Size = new System.Drawing.Size(360, 272);
            this.GroupBoxFolderLocations.TabIndex = 12;
            this.GroupBoxFolderLocations.TabStop = false;
            this.GroupBoxFolderLocations.Text = "Folder Scanning";
            // 
            // ButtonClickToAdd
            // 
            this.ButtonClickToAdd.Location = new System.Drawing.Point(244, 16);
            this.ButtonClickToAdd.Name = "ButtonClickToAdd";
            this.ButtonClickToAdd.Size = new System.Drawing.Size(79, 29);
            this.ButtonClickToAdd.TabIndex = 5;
            this.ButtonClickToAdd.Text = "Add this";
            this.ButtonClickToAdd.UseVisualStyleBackColor = true;
            this.ButtonClickToAdd.Click += new System.EventHandler(this.button1_Click);
            // 
            // ButtonSelectAllCheckedFolderList
            // 
            this.ButtonSelectAllCheckedFolderList.Location = new System.Drawing.Point(142, 220);
            this.ButtonSelectAllCheckedFolderList.Name = "ButtonSelectAllCheckedFolderList";
            this.ButtonSelectAllCheckedFolderList.Size = new System.Drawing.Size(114, 32);
            this.ButtonSelectAllCheckedFolderList.TabIndex = 4;
            this.ButtonSelectAllCheckedFolderList.Text = "Check All";
            this.ButtonSelectAllCheckedFolderList.UseVisualStyleBackColor = true;
            // 
            // ButtonClearCheckedFolderList
            // 
            this.ButtonClearCheckedFolderList.Location = new System.Drawing.Point(10, 220);
            this.ButtonClearCheckedFolderList.Name = "ButtonClearCheckedFolderList";
            this.ButtonClearCheckedFolderList.Size = new System.Drawing.Size(125, 32);
            this.ButtonClearCheckedFolderList.TabIndex = 3;
            this.ButtonClearCheckedFolderList.Text = "Clear Checked";
            this.ButtonClearCheckedFolderList.UseVisualStyleBackColor = true;
            this.ButtonClearCheckedFolderList.Click += new System.EventHandler(this.ButtonClearCheckedFolderList_Click);
            // 
            // ButtonBrowseForNewFolder
            // 
            this.ButtonBrowseForNewFolder.Location = new System.Drawing.Point(197, 17);
            this.ButtonBrowseForNewFolder.Name = "ButtonBrowseForNewFolder";
            this.ButtonBrowseForNewFolder.Size = new System.Drawing.Size(41, 26);
            this.ButtonBrowseForNewFolder.TabIndex = 2;
            this.ButtonBrowseForNewFolder.Text = "...";
            this.ButtonBrowseForNewFolder.UseVisualStyleBackColor = true;
            this.ButtonBrowseForNewFolder.Click += new System.EventHandler(this.ButtonBrowseForNewFolder_Click);
            // 
            // TextBoxFreeFormFolder
            // 
            this.TextBoxFreeFormFolder.Location = new System.Drawing.Point(10, 17);
            this.TextBoxFreeFormFolder.Name = "TextBoxFreeFormFolder";
            this.TextBoxFreeFormFolder.Size = new System.Drawing.Size(181, 26);
            this.TextBoxFreeFormFolder.TabIndex = 1;
            this.TextBoxFreeFormFolder.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBoxFreeFormFolder_KeyPress);
            this.TextBoxFreeFormFolder.MouseLeave += new System.EventHandler(this.TextBoxFreeFormFolder_MouseLeave);
            this.TextBoxFreeFormFolder.MouseHover += new System.EventHandler(this.TextBoxFreeFormFolder_MouseHover);
            // 
            // checkedListBoxFolderSelection
            // 
            this.checkedListBoxFolderSelection.FormattingEnabled = true;
            this.checkedListBoxFolderSelection.Location = new System.Drawing.Point(10, 49);
            this.checkedListBoxFolderSelection.Name = "checkedListBoxFolderSelection";
            this.checkedListBoxFolderSelection.Size = new System.Drawing.Size(165, 165);
            this.checkedListBoxFolderSelection.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.CheckBoxIsTopLevelGroup);
            this.groupBox5.Controls.Add(this.CheckBoxIsDisabled);
            this.groupBox5.Location = new System.Drawing.Point(17, 141);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(321, 100);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Global Setting for this Group";
            // 
            // CheckBoxIsTopLevelGroup
            // 
            this.CheckBoxIsTopLevelGroup.AutoSize = true;
            this.CheckBoxIsTopLevelGroup.Location = new System.Drawing.Point(6, 25);
            this.CheckBoxIsTopLevelGroup.Name = "CheckBoxIsTopLevelGroup";
            this.CheckBoxIsTopLevelGroup.Size = new System.Drawing.Size(169, 24);
            this.CheckBoxIsTopLevelGroup.TabIndex = 7;
            this.CheckBoxIsTopLevelGroup.Text = "Is Top Level Group";
            this.CheckBoxIsTopLevelGroup.UseVisualStyleBackColor = true;
            // 
            // CheckBoxIsDisabled
            // 
            this.CheckBoxIsDisabled.AutoSize = true;
            this.CheckBoxIsDisabled.Location = new System.Drawing.Point(6, 55);
            this.CheckBoxIsDisabled.Name = "CheckBoxIsDisabled";
            this.CheckBoxIsDisabled.Size = new System.Drawing.Size(114, 24);
            this.CheckBoxIsDisabled.TabIndex = 10;
            this.CheckBoxIsDisabled.Text = "Is Disabled";
            this.CheckBoxIsDisabled.UseVisualStyleBackColor = true;
            // 
            // GroupIconSetting
            // 
            this.GroupIconSetting.Controls.Add(this.CheckBoxNoGroupIcon);
            this.GroupIconSetting.Controls.Add(this.ButtonBrowseForGroupIcon);
            this.GroupIconSetting.Controls.Add(this.TextBoxGroupIconSource);
            this.GroupIconSetting.Controls.Add(this.LabeGrouplconSource);
            this.GroupIconSetting.Location = new System.Drawing.Point(17, 24);
            this.GroupIconSetting.Name = "GroupIconSetting";
            this.GroupIconSetting.Size = new System.Drawing.Size(321, 111);
            this.GroupIconSetting.TabIndex = 7;
            this.GroupIconSetting.TabStop = false;
            this.GroupIconSetting.Text = "Group Icon Settings";
            // 
            // CheckBoxNoGroupIcon
            // 
            this.CheckBoxNoGroupIcon.AutoSize = true;
            this.CheckBoxNoGroupIcon.Location = new System.Drawing.Point(156, 29);
            this.CheckBoxNoGroupIcon.Name = "CheckBoxNoGroupIcon";
            this.CheckBoxNoGroupIcon.Size = new System.Drawing.Size(90, 24);
            this.CheckBoxNoGroupIcon.TabIndex = 6;
            this.CheckBoxNoGroupIcon.Text = "No Icon";
            this.CheckBoxNoGroupIcon.UseVisualStyleBackColor = true;
            this.CheckBoxNoGroupIcon.CheckedChanged += new System.EventHandler(this.CheckBoxNoGroupIcon_CheckedChanged);
            // 
            // ButtonBrowseForGroupIcon
            // 
            this.ButtonBrowseForGroupIcon.Location = new System.Drawing.Point(223, 56);
            this.ButtonBrowseForGroupIcon.Name = "ButtonBrowseForGroupIcon";
            this.ButtonBrowseForGroupIcon.Size = new System.Drawing.Size(46, 27);
            this.ButtonBrowseForGroupIcon.TabIndex = 5;
            this.ButtonBrowseForGroupIcon.Text = "...";
            this.ButtonBrowseForGroupIcon.UseVisualStyleBackColor = true;
            // 
            // TextBoxGroupIconSource
            // 
            this.TextBoxGroupIconSource.Location = new System.Drawing.Point(6, 56);
            this.TextBoxGroupIconSource.Name = "TextBoxGroupIconSource";
            this.TextBoxGroupIconSource.Size = new System.Drawing.Size(211, 26);
            this.TextBoxGroupIconSource.TabIndex = 4;
            // 
            // LabeGrouplconSource
            // 
            this.LabeGrouplconSource.AutoSize = true;
            this.LabeGrouplconSource.Location = new System.Drawing.Point(6, 33);
            this.LabeGrouplconSource.Name = "LabeGrouplconSource";
            this.LabeGrouplconSource.Size = new System.Drawing.Size(144, 20);
            this.LabeGrouplconSource.TabIndex = 3;
            this.LabeGrouplconSource.Text = "Group Icon Source";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.TabPageGroupEditorTab);
            this.tabControl1.Controls.Add(this.TabPagePolicyLevelSettings);
            this.tabControl1.Controls.Add(this.TabPageTraySettings);
            this.tabControl1.Location = new System.Drawing.Point(101, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(751, 520);
            this.tabControl1.TabIndex = 0;
            // 
            // TabPagePolicyLevelSettings
            // 
            this.TabPagePolicyLevelSettings.Location = new System.Drawing.Point(4, 29);
            this.TabPagePolicyLevelSettings.Name = "TabPagePolicyLevelSettings";
            this.TabPagePolicyLevelSettings.Padding = new System.Windows.Forms.Padding(3);
            this.TabPagePolicyLevelSettings.Size = new System.Drawing.Size(743, 487);
            this.TabPagePolicyLevelSettings.TabIndex = 2;
            this.TabPagePolicyLevelSettings.Text = "Policy Level Settings";
            this.TabPagePolicyLevelSettings.UseVisualStyleBackColor = true;
            // 
            // TabPageTraySettings
            // 
            this.TabPageTraySettings.Location = new System.Drawing.Point(4, 29);
            this.TabPageTraySettings.Name = "TabPageTraySettings";
            this.TabPageTraySettings.Size = new System.Drawing.Size(743, 487);
            this.TabPageTraySettings.TabIndex = 3;
            this.TabPageTraySettings.Text = "tabPage1";
            this.TabPageTraySettings.UseVisualStyleBackColor = true;
            // 
            // ListBoxGroupList
            // 
            this.ListBoxGroupList.FormattingEnabled = true;
            this.ListBoxGroupList.ItemHeight = 20;
            this.ListBoxGroupList.Location = new System.Drawing.Point(11, 12);
            this.ListBoxGroupList.Name = "ListBoxGroupList";
            this.ListBoxGroupList.Size = new System.Drawing.Size(84, 504);
            this.ListBoxGroupList.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 522);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 41);
            this.button1.TabIndex = 2;
            this.button1.Text = "New Group";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // GroupEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 575);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ListBoxGroupList);
            this.Controls.Add(this.tabControl1);
            this.Name = "GroupEditorForm";
            this.Text = "CorpTray Editor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.TabPageGroupEditorTab.ResumeLayout(false);
            this.GroupBoxNames.ResumeLayout(false);
            this.GroupBoxNames.PerformLayout();
            this.GroupBoxFolderLocations.ResumeLayout(false);
            this.GroupBoxFolderLocations.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.GroupIconSetting.ResumeLayout(false);
            this.GroupIconSetting.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip ToolTipSettingHelpShow;
        private System.Windows.Forms.TabPage TabPageGroupEditorTab;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.GroupBox GroupBoxNames;
        private System.Windows.Forms.TextBox TextBoxGroupBoxName;
        private System.Windows.Forms.Label LabelGroupBoxNameText;
        private System.Windows.Forms.GroupBox GroupBoxFolderLocations;
        private System.Windows.Forms.Button ButtonSelectAllCheckedFolderList;
        private System.Windows.Forms.Button ButtonClearCheckedFolderList;
        private System.Windows.Forms.Button ButtonBrowseForNewFolder;
        private System.Windows.Forms.TextBox TextBoxFreeFormFolder;
        private System.Windows.Forms.CheckedListBox checkedListBoxFolderSelection;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox CheckBoxIsTopLevelGroup;
        private System.Windows.Forms.CheckBox CheckBoxIsDisabled;
        private System.Windows.Forms.GroupBox GroupIconSetting;
        private System.Windows.Forms.CheckBox CheckBoxNoGroupIcon;
        private System.Windows.Forms.Button ButtonBrowseForGroupIcon;
        private System.Windows.Forms.TextBox TextBoxGroupIconSource;
        private System.Windows.Forms.Label LabeGrouplconSource;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialogSelectLinkFolder;
        private System.Windows.Forms.ToolTip ToolTipShowFolderLocation;
        private System.Windows.Forms.Button ButtonClickToAdd;
        private System.Windows.Forms.TabPage TabPagePolicyLevelSettings;
        private System.Windows.Forms.TabPage TabPageTraySettings;
        private System.Windows.Forms.ListBox ListBoxGroupList;
        private System.Windows.Forms.Button button1;
    }
}

