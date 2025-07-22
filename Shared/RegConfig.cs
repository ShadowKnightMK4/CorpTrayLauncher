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

using CorpTrayLauncher.IconHandling;
using CorpTrayLauncher.RegistryHandling;
namespace CorpTrayLauncher.RegistryHandling
{
//#error What needs to be done next is ensure the Policy LOCAL_MACHINE_KEY and User CURRENT_USER_KEY play nice.
//#error The code currently DOES NOT CHECK policy setting for groups. That's next.

    
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

        static void HandleFolderShortcutToolMenu_branching(DirectoryInfo Source, ContextMenuStrip Target, ToolStripMenuItem OtherTarget, IResolveIconFromPath IconHandler)
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
                        NewItem.Image = IconHandler.ResolveIcon(linkObject);
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

        static void AddGroupToMenu(ToolStripMenuItem Ext, RegGroup Group, IResolveIconFromPath IconHandler)
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
                    HandleFolderShortcutToolMenu_branching(Folders[i], null, Ext, IconHandler);
                }
            }
        }
        static void AddGroupToMenu(ContextMenuStrip Ext, RegGroup Group, IResolveIconFromPath Iconhandler)
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
                    HandleFolderShortcutToolMenu_branching(Folders[i], Ext, null, Iconhandler);
                }
            }
        }
        /// <summary>
        /// Parse, extract and handle the group to the tool menu.
        /// </summary>
        /// <param name="Ext"></param>
        /// <param name="Group"></param>
        public static void AddGroup(this ContextMenuStrip Ext, RegGroup Group, IResolveIconFromPath Resolver)
        {
           
            if (!Group.IsTopLevel)
            {
                ToolStripMenuItem ToolStrip;
                DebugStuff.WriteLog("Adding group " + Group.Name + " to context menu as a submenu.");
                ToolStrip =  new ToolStripMenuItem();
                AddGroupToMenu(ToolStrip ,Group, Resolver);
                ToolStrip.Text = Group.Name;
                ToolStrip.Image = Group.GetGroupIcon();
                Ext.Items.Add(ToolStrip);
            }
            else
            {
                DebugStuff.WriteLog("Adding group " + Group.Name + " to context menu as a top level collection.");
                AddGroupToMenu(Ext, Group, Resolver);
      
            }


            
        
        }
    }
    /// <summary>
    /// Represents a read only view of a reg group
    /// </summary>
    internal class RegGroup :IDisposable
    {
        readonly string name;
        readonly IResolveIconFromPath IconHandler;
        readonly IRegProvider Reg;
        RegistryKey GroupKey;
        
        public  RegSettings2.GroupType GetGroupType()
        { 
            if (GroupKey != null)
            {
                if (GroupKey.Name.Contains(RegConfigLocations.UserGroupLocation))
                {
                    return RegSettings2.GroupType.User;
                }
                else if (GroupKey.Name.Contains(RegConfigLocations.PolicySettingLocation))
                {
                    return RegSettings2.GroupType.Policy;
                }
            }
            return RegSettings2.GroupType.None;
        }
        /// <summary>
        /// Create a group class to ready this reg key entry, Handler will resolve icons. If blank, an <see cref="IconResolverHandler"/> is used
        /// </summary>
        /// <param name="FromName">open this group</param>
        /// <param name="Handler">handler to resolving icons</param>
        public RegGroup(string FromName, IResolveIconFromPath Handler)
        {
            Reg = new DefaultRegProvider(); // Default registry provider if none provided
            /* our plan 
             * is to first try opening as a policy group. If that's blocked, user group
             * */
            try
            {
                if (string.IsNullOrEmpty(FromName))
                {
                    throw new ArgumentException("Group name cannot be null or empty", nameof(FromName));
                }
            }
            catch (ArgumentException ex)
            {
                DebugStuff.WriteLog("Error creating RegGroup: " + ex.Message);
                throw; // Re-throw the exception to be handled by the caller
            }

            try
            {
                GroupKey = Reg.OpenRegKey(Registry.LocalMachine, RegConfigLocations.UserGroupLocation + "\\" + FromName, false);
            }
            catch (UnauthorizedAccessException e)
            {
                DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                DebugStuff.WriteLog("Skipping trying to load from policy.");
                GroupKey = null;
            }
            catch (Exception e)
            {
                DebugStuff.WriteLog("Error to open registry key: " + e.Message);
                DebugStuff.WriteLog("Skipping trying to load from policy.");
                GroupKey = null;
            }

            if (GroupKey == null)
            {
                try
                {
                    GroupKey = Reg.OpenRegKey(Registry.CurrentUser, RegConfigLocations.UserGroupLocation + "\\" + FromName, false);
                }
                catch (AccessViolationException e)
                {
                    DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                    throw;
                }
            }


         //GroupKey = Registry.CurrentUser.OpenSubKey(RegConfigLocations.UserGroupLocation + "\\" + FromName);
            if (GroupKey == null)
            {
                GroupKey = Reg.CreateSubKey(Registry.CurrentUser, RegConfigLocations.UserGroupLocation + "\\" + FromName);
                if (GroupKey == null)
                {
                    throw new InvalidOperationException("Failed to create or open registry key for group: " + FromName);
                }
            }
            name = FromName;
            if (Handler == null)
            {
                IconHandler = new IconResolverHandler(); // Default icon handler if none provided
            }
            else
            {
                IconHandler = Handler; // Use the provided icon handler
            }
        }

        public RegGroup(string FromName, IResolveIconFromPath Handler, IRegProvider RegistryProvider)
        {
            if (RegistryProvider == null)
            {
                throw new ArgumentNullException(nameof(Registry), "Registry provider cannot be null");
            }
            Reg = RegistryProvider;
            try
            {
                if (string.IsNullOrEmpty(FromName))
                {
                    throw new ArgumentException("Group name cannot be null or empty", nameof(FromName));
                }
            }
            catch (ArgumentException ex)
            {
                DebugStuff.WriteLog("Error creating RegGroup: " + ex.Message);
                throw; // Re-throw the exception to be handled by the caller
            }

            try
            {
                GroupKey = Reg.OpenRegKey(Registry.LocalMachine, RegConfigLocations.UserGroupLocation + "\\" + FromName, false);
            }
            catch (AccessViolationException e)
            {
                DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                DebugStuff.WriteLog("Skipping trying to load from policy.");
            }

            if (GroupKey == null)
            {
                try
                {
                    GroupKey = Reg.OpenRegKey(Registry.CurrentUser, RegConfigLocations.UserGroupLocation + "\\" + FromName, false);
                }
                catch (AccessViolationException e)
                {
                    DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                    throw;
                }
            }


            //GroupKey = Registry.CurrentUser.OpenSubKey(RegConfigLocations.UserGroupLocation + "\\" + FromName);
            if (GroupKey == null)
            {
                GroupKey = Reg.CreateSubKey(Registry.CurrentUser, RegConfigLocations.UserGroupLocation + "\\" + FromName);
                if (GroupKey == null)
                {
                    throw new InvalidOperationException("Failed to create or open registry key for group: " + FromName);
                }
            }
            name = FromName;
            if (Handler == null)
            {
                IconHandler = new IconResolverHandler(); // Default icon handler if none provided
            }
            else
            {
                IconHandler = Handler; // Use the provided icon handler
            }
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

        /// <summary>d
        /// Get the image if any defined for this group. This is used to display the icon in the context menu. Blank is fine.
        /// </summary>
        /// <returns></returns>
        public Image GetGroupIcon()
        {
            if (GroupKey != null)
            {
                //object value = GroupKey.GetValue(RegConfigLocations.TrayIcon);
                object value = this.Reg.GetValue(GroupKey, null, RegConfigLocations.TrayIcon, null);
                if (value != null && value is string iconPath && !string.IsNullOrEmpty(iconPath))
                {
                    Image icon = IconHandler.ResolveIcon(value.ToString());
                    return icon;
                }
            }
            return null; // No icon defined, return null
        }



        /// <summary>
        /// Grab all directories out of the folder exist. Should the registry key not exist, it will return the default directories.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Default Directory list is exe running path and a subfolder of that "Itmes"</remarks>
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



 

       
    
}
