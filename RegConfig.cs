using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using Shell32;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace CorpTrayLauncher
{

    internal static class RegConfigLocations
    {
        /// <summary>
        /// Open the registry hive that's the user group storage location
        /// </summary>
        /// <returns></returns>
        public static  RegistryKey OpenUsersGroup()
        {
            return Registry.CurrentUser.OpenSubKey(UserGroupLocation, true) ?? Registry.CurrentUser.CreateSubKey(UserGroupLocation);
        }

        public static RegistryKey OpenPolicyGroup()
        {
            return Registry.LocalMachine.OpenSubKey(PolicySettingLocation, true) ?? Registry.LocalMachine.CreateSubKey(PolicySettingLocation);
        }

        /// <summary>
        /// Grab the custom tray icon regkey
        /// </summary>
        /// <returns></returns>
        public static RegistryKey OpenTrayIconSetting()
        {
            RegistryKey policy = Registry.CurrentUser.OpenSubKey(PolicyTrayIconLocation, true);
            RegistryKey user = Registry.CurrentUser.OpenSubKey(UserTrayIconLocation, true);

            if (policy != null && user != null)
            {
                user.Dispose();
                return policy; // Policy overrides user settings
            }
            else if (user != null)
            {
                return user; // Only user settings available
            }
            else
            {
                return null; // No settings found
            }
        }
        public const string PolicySettingLocation = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\CorpTrayLauncher";
        public const string UserSettingLocation = "HKEY_CURRENT_USER\\Software\\CorpTrayLauncher\\";
        
        public const string UserTrayIconLocation = "\\Software\\CorpTrayLauncher\\TrayIcon";
        public const string PolicyTrayIconLocation = "\\Software\\Policies\\CorpTrayLauncher\\TrayIcon";

        public const string UserGroupLocation = "Software\\CorpTrayLauncher\\Groups";
        /// <summary>
        /// Name of registry key where the folder paths are stored.
        /// </summary>
        public const string FolderRegValueName = "FolderBuildingPath";
        public const string DisabledRegValueName = "Disabled";
        public const string IsTopLevelRegValueName = "IsTopLevel"; // This is used to determine if the group is a top-level group or a sub-group.
        public const string TrayIcon = "TrayIcon"; // This is used to store the icon for the group.
    }

    static class LinkDispatch
    {
       public static void FollowLink(ShellLinkObject obj)
        {
            Process StartMe = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = obj.Path,
                    Arguments = obj.Arguments,
                    WorkingDirectory = obj.WorkingDirectory,
                    UseShellExecute = true
                }
            };
            try
            {
                StartMe?.Start();
            }
            catch (Exception ex)
            {
                DebugStuff.WriteLog("Error starting process: " + ex.Message);
                MessageBox.Show($"Error launching shortcut: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Exit if there's an error starting the process
            }
            finally
            {
                StartMe?.Dispose();
            }
                return;
        }
    }

    static class ToolStripExt
    {
       static Shell Help = null;

        /// <summary>
        /// Assign a name to the item based on the target file name. This is used when the link does not have a description set.
        /// </summary>
        /// <param name="Target"></param>
        /// <returns></returns>
        static string AssignFromTarget(string Target)
        {
            string trim = Path.GetFileNameWithoutExtension(Target);
            return trim;

        }

        static void HandleFolderShortcutToolMenu_branching(DirectoryInfo Source, ContextMenuStrip Target, ToolStripMenuItem OtherTarget)
        {
            if (Help == null)
            {
                DebugStuff.WriteLog("Creating new Shell instance (.NET) for lnk file handling");
                try
                {
                    Help = new Shell();
                }
                catch (Exception ex)
                {
                    DebugStuff.WriteLog("Error creating Shell instance: " + ex.Message);
                    MessageBox.Show("Error initializing shell: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                DebugStuff.WriteLog("Using existing Shell instance (.NET) for lnk file handling from old processin session.");
            }
            
            var Items = Source.GetFiles("*.lnk", SearchOption.TopDirectoryOnly);
            if (Items == null || Items.Length == 0)
            {
                DebugStuff.WriteLog("No .lnk files found in " + Source.FullName);
                return; // No items to process
            }
            else
            {
                DebugStuff.WriteLog("Found " + Items.Length + " .lnk files in " + Source.FullName);
            }
                foreach (var Item in Items)
                {
                    FolderItem shellitem = Help.NameSpace(Source.FullName).ParseName(Item.Name);
                    if (shellitem.IsLink)
                    {

                        ToolStripItem NewItem = new ToolStripMenuItem();
                    ShellLinkObject linkObject = null;
                        try
                        {
                         linkObject = shellitem.GetLink as ShellLinkObject;
                        }
                        catch (Exception ex)
                        {
                            DebugStuff.WriteLog("Error getting link object: " + ex.Message);
                            MessageBox.Show($"Error processing shortcut '{Item.Name}': {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue; // Skip this item if there's an error
                        }
                           NewItem.Text = linkObject?.Description ?? Item.Name;
                        if (string.IsNullOrEmpty(NewItem.Text))
                        {
                            NewItem.Text = AssignFromTarget(linkObject.Path);
                        }
                        NewItem.Image = IconResolver.ResolveIcon(linkObject);
                        NewItem.Tag = linkObject;
                        NewItem.Click += (sender, e) =>
                        {

                            try
                            {
                                LinkDispatch.FollowLink(((ToolStripItem)sender).Tag as ShellLinkObject);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error launching shortcut: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        };
                    DebugStuff.WriteLog("Adding item " + NewItem.Text + " to context menu from folder " + Source.FullName);
                    Target?.Items.Add(NewItem);
                        OtherTarget?.DropDown.Items.Add(NewItem);
                    }

                }
        }

        static void AddGroupToMenu(ToolStripMenuItem Ext, RegGroup Group)
        {
            if (Group == null || !Group.IsEnabled)
            {
                return; // If the group is null or not enabled, do not add it.
            }
            var Folders = Group.GetDirectories();
            for (int i = 0; i < Folders.Count; i++)
            {
                
                if (Folders[i] == null || !Folders[i].Exists)
                {
                    Folders.RemoveAt(i);
                    i--;
                }
                else
                {
                    DebugStuff.WriteLog("Adding folder " + Folders[i].FullName + " to context menu.");
                    HandleFolderShortcutToolMenu_branching(Folders[i], null, Ext);
                }
            }
        }
        static void AddGroupToMenu(ContextMenuStrip Ext, RegGroup Group)
        {
            if (Group == null || !Group.IsEnabled)
            {
                return; // If the group is null or not enabled, do not add it.
            }
            var Folders = Group.GetDirectories();
            for (int i = 0; i < Folders.Count; i++)
            {
                if (Folders[i] == null || !Folders[i].Exists)
                {
                    Folders.RemoveAt(i);
                    i--;
                }
                else
                {
                    DebugStuff.WriteLog("Adding folder " + Folders[i].FullName + " to context menu.");
                    HandleFolderShortcutToolMenu_branching(Folders[i], Ext, null);
                }
            }
        }
        /// <summary>
        /// Parse, extract and handle the group to the tool menu.
        /// </summary>
        /// <param name="Ext"></param>
        /// <param name="Group"></param>
        public static void AddGroup(this ContextMenuStrip Ext, RegGroup Group)
        {
           
            if (!Group.IsTopLevel)
            {
                ToolStripMenuItem ToolStrip;
                DebugStuff.WriteLog("Adding group " + Group.Name + " to context menu as a submenu.");
                ToolStrip =  new ToolStripMenuItem();
                AddGroupToMenu(ToolStrip ,Group);
                ToolStrip.Text = Group.Name;
                ToolStrip.Image = Group.GetGroupIcon();
                Ext.Items.Add(ToolStrip);
            }
            else
            {
                DebugStuff.WriteLog("Adding group " + Group.Name + " to context menu as a top level collection.");
                AddGroupToMenu(Ext, Group);
      
            }


            
        
        }
    }
    /// <summary>
    /// Represents a read only view of a reg group
    /// </summary>
    internal class RegGroup :IDisposable
    {
        readonly string name;
        RegistryKey GroupKey;
        /// <summary>
        /// Create or open a group 
        /// </summary>
        /// <param name="FromName"></param>
        public RegGroup(string FromName)
        {
            GroupKey = Registry.CurrentUser.OpenSubKey(RegConfigLocations.UserGroupLocation + "\\" + FromName);
            if (GroupKey == null)
            {
                GroupKey = Registry.CurrentUser.CreateSubKey(RegConfigLocations.UserGroupLocation + "\\" + FromName);
            }
            name = FromName;
        }

        /// <summary>
        /// Get the name of the group ie registry get we created this class from.
        /// </summary>
        public string Name => name;


        /// <summary>
        /// return if this group of icons are top level or not. Top level groups are the ones that are displayed in the main menu, while sub-groups are displayed in the sub-menus.
        /// </summary>
        public bool IsTopLevel
        {
            get
            {
                if (GroupKey != null)
                {
                    object value = GroupKey.GetValue(RegConfigLocations.IsTopLevelRegValueName);
                    if (value != null && value is int intValue)
                    {
                        return intValue == 1; // 1 means true, 0 means false
                    }
                }
                return false; // Default to false if not set
            }
        }
        /// <summary>
        /// Return if this particular group is enabled or not. On refreshing menu. anything not enabled is skipped
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (GroupKey != null)
                {
                    object value = GroupKey.GetValue(RegConfigLocations.DisabledRegValueName);
                    if (value != null && value is int intValue)
                    {
                        return intValue == 0;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Get the image if any defined for this group. This is used to display the icon in the context menu. Blank is fine.
        /// </summary>
        /// <returns></returns>
        public Image GetGroupIcon()
        {
            if (GroupKey != null)
            {
                object value = GroupKey.GetValue(RegConfigLocations.TrayIcon);
                if (value != null && value is string iconPath && !string.IsNullOrEmpty(iconPath))
                {
                    Image icon = IconResolver.ResolveIcon(value.ToString());
                    return icon;
                }
            }
            return null; // No icon defined, return null
        }



        /// <summary>
        /// Grab all directories out of the folder exist. Should the registry key not exist, it will return the default directories.
        /// </summary>
        /// <returns></returns>
        public List<DirectoryInfo> GetDirectories()
        {
            List<DirectoryInfo> directories = new List<DirectoryInfo>();
            if (GroupKey != null)
            {
                string[] paths = GroupKey.GetValue(RegConfigLocations.FolderRegValueName)?.ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (paths != null && paths.Length > 0)
                {
                    directories = paths.Select(path => new DirectoryInfo(path.Trim())).ToList();
                    // Filter out directories that do not exist
                    directories = directories.Where(dir => dir.Exists).ToList();
                }
                else
                {
                    // If no paths are set, return default paths
                    directories.Add(new DirectoryInfo(Directory.GetCurrentDirectory()));
                    directories.Add(new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Items")));
                }
            }
            else
            {
                // If the group key does not exist, return an empty list
                directories = new List<DirectoryInfo>();
            }
            return directories;
        }

        /// <summary>
        /// Clean up!
        /// </summary>
        public void Dispose()
        {
            if (GroupKey != null)
            {
                GroupKey.Dispose();
                GroupKey = null;
            }
            GC.SuppressFinalize(this);
        }
    }



    internal static class RegSettingChecker
    {
        /// <summary>
        /// How this works. Pass Policy+User, BOTH are checked.  should they be in conflict, Policy negative wins ie. Example: if a group1 is defined as active at user but disabled at policy - group1 is disabled.. Likewise if group1 is disabled at user level but alive at polcy, it's active
        /// </summary>
        enum SettingCheckMode
        {
            Policy,
            User
        }

        public static bool UserGroupExists(string name)
        {
            using (RegistryKey key = RegConfigLocations.OpenUsersGroup())
            {
                if (key != null)
                {
                    if (key.SubKeyCount > 0)
                    {
                        // Check if the specific group exists in the user registry
                        return key.GetSubKeyNames().Contains(name);
                    }
                }
            }
            return false;
        }


        public static bool IsUserGroupActive(string name)
        {
            using (RegistryKey key = RegConfigLocations.OpenUsersGroup())
            {
                if (key != null)
                {
                    if (key.GetSubKeyNames().Contains(name))
                    {
                        object value = key.GetValue(name + "\\" + RegConfigLocations.DisabledRegValueName);
                        if (value != null && value is int intValue)
                        {
                            return intValue == 0; // 0 means enabled, 1 means disabled
                        }
                    }
                }
            }
            return true; // Default to false if not set
        }
       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static List<string> GetActiveGroupsNames(List<string> groups)
        {
            using (RegistryKey key = RegConfigLocations.OpenUsersGroup())
            {
                if (key != null)
                {
                    foreach (string groupName in key.GetSubKeyNames())
                    {
                        if (IsUserGroupActive(groupName))
                        {
                            groups.Add(groupName);
                        }
                    }
                    return groups;
                }
            }
            return null;
        }
        public static List<string> GetActiveGroupsNames()
        {
            var ret = new List<string>();
            return GetActiveGroupsNames(ret);
        }

        public static List<RegGroup> GetGroups(List<string> groups)
        {
            if (groups == null || groups.Count == 0)
            {
                groups = GetActiveGroupsNames();
            }
            List<RegGroup> groupList = new List<RegGroup>();
            foreach (string groupName in groups)
            {

                    RegGroup group = new RegGroup(groupName);
                    if (group.IsEnabled)
                    {
                        groupList.Add(group);
                    }
            }
            return groupList;
        }


        
    }

       
    
}
