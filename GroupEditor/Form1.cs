using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CorpTrayLauncher.RegistryHandling;
namespace GroupEditor
{
    public partial class GroupEditorForm : Form
    {
          RegSettings2 regSettings;
        public GroupEditorForm()
        {
            InitializeComponent();
        }

        private void checkBoxRefreshMenu_MouseHover(object sender, EventArgs e)
        {
            //ToolTipSettingHelpShow.Show(ToolTipHelpText.ToolTipRefreshHelp, checkBoxRefreshMenu, MousePosition.X, MousePosition.Y + 20, ToolTipHelpText.ToolTipRefreshTimeout);
        }

        private void checkBoxExitAppButton_MouseHover(object sender, EventArgs e)
        {
            //ToolTipSettingHelpShow.Show(ToolTipHelpText.ToolTopExitButtonHelp, checkBoxExitAppButton, MousePosition.X, MousePosition.Y + 20, ToolTipHelpText.ToolTipRefreshTimeout);
        }

        private void checkBoxExitAppButton_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void tabPageUserSetting_Click(object sender, EventArgs e)
        {

        }

        private void CheckBoxNoGroupIcon_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxNoGroupIcon.Checked)
            {
                // Disable the group icon setting
                TextBoxGroupIconSource.Enabled = false;
                ButtonBrowseForGroupIcon.Enabled = false;
            }
            else
            {
                // Enable the group icon setting
                TextBoxGroupIconSource.Enabled = true;
                ButtonBrowseForGroupIcon.Enabled = true;
            }
        }

        static bool ValidateGroupName(string groupName, bool Complain)
        {
            // Check if the group name is empty
            if (string.IsNullOrWhiteSpace(groupName))
            {
                if (Complain) MessageBox.Show("Group name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (groupName.Contains('\\') || groupName.Contains('/'))
            {
                if (Complain) MessageBox.Show("Group name cannot contain slashes.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (regSettings == null)
            {
                regSettings = new RegSettings2();
            }
            var Groups = regSettings.GetGroupNames(false);
            foreach (var group in Groups)
            {
                ListBoxGroupList.Items.Add(group);
            }
            ListBoxGroupList.Sorted = true;
            ListBoxGroupList.Sorted = false;
        }

        private void TextBoxGroupBoxName_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !ValidateGroupName(TextBoxGroupBoxName.Text, true);
            if (e.Cancel)
            {
                TextBoxGroupBoxName.SelectAll();
            }
        }

        private void ButtonBrowseForNewFolder_Click(object sender, EventArgs e)
        {
            if (FolderBrowserDialogSelectLinkFolder.ShowDialog(this) == DialogResult.OK)
            {
                TextBoxFreeFormFolder.Text = FolderBrowserDialogSelectLinkFolder.SelectedPath;
            }
        }

        private void TextBoxFreeFormFolder_MouseLeave(object sender, EventArgs e)
        {

        }

        private void TextBoxFreeFormFolder_MouseHover(object sender, EventArgs e)
        {
            ToolTipShowFolderLocation.Show(TextBoxFreeFormFolder.Text, TextBoxFreeFormFolder, MousePosition.X, MousePosition.Y + 20, ToolTipHelpText.ToolTipRefreshTimeout);
        }

        
        private void TextBoxFreeFormFolder_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                var d = TextBoxFreeFormFolder.Text.Trim();
                if (!string.IsNullOrEmpty(d) && (Directory.Exists(d)))
                {
                    checkedListBoxFolderSelection.Items.Add(TextBoxFreeFormFolder.Text, true);
                }
                
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var d = TextBoxFreeFormFolder.Text.Trim();
            if (!string.IsNullOrEmpty(d) && (Directory.Exists(d)))
            {
                checkedListBoxFolderSelection.Items.Add(TextBoxFreeFormFolder.Text, true);
            }

        }

        private void ButtonClearCheckedFolderList_Click(object sender, EventArgs e)
        {
            //checkedListBoxFolderSelection
        }
    }

    static class ToolTipHelpText
    {
        public const int ToolTipRefreshTimeout = 400;
        public const string ToolTipRefreshHelp = "Add's refresh menu button to the right click menu. Will rescan shortcut sources.";
        public const string ToolTopExitButtonHelp = "Add's exit app button to the right click menu. Will close the app when clicked.";
    }
}
