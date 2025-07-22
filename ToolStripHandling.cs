using CorpTrayLauncher.IconHandling;
using CorpTrayLauncher.RegistryHandling;
using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CorpTrayLauncher.Shortcuts;
namespace CorpTrayLauncher
{

    static class RegGroupDispatcher
    {
        public static void AddGroups(List<string> Groups, IResolveIconFromPath IconResolverHandler,  IFolderProviderResolver FolderResolver, ContextMenuStrip RightClickContextMenuStrip, out int active_count)
        {

            active_count = 0; 
            
            if (IconResolverHandler != null)
            {
                DebugStuff.WriteLog("Begining adding Group, valid IconResolveFromPath interface. System will try resolving icon to menu item.");
            }
            else
            {
                DebugStuff.WriteLog("Begining adding Group, null IconResolveFromPath interface. System will NOT resolve icons to menu item");
            }

            if ( (Groups == null) || (Groups.Count == 0))
            {
                DebugStuff.WriteLog("No groups added cause empty Group or null group passed to RegGroupDispatcher.");
                return;
            }
                for (int i = 0; i < Groups.Count; i++)
                {
                    var group = Groups[i];
                    if (!string.IsNullOrWhiteSpace(group))
                    {
                        RegGroup regGroup = new WritableRegGroup(group, IconResolverHandler, FolderResolver);
                        if (!regGroup.IsEnabled)
                        {
                            string msg = string.Empty;
                            switch (regGroup.GetGroupType())
                            {
                            case RegSettings2.GroupType.None:
                                msg += "Unknown "; break;
                            case RegSettings2.GroupType.User:
                                msg += "User ";  break;
                            case RegSettings2.GroupType.Policy:
                                msg += "Policy "; break;
                            }
                            msg += $"Group {group} is not active, skipping.";
        

                            DebugStuff.WriteLog(msg);
                            regGroup.Dispose();
                            regGroup = null; // clear the reference to the group
                            continue;
                        }
                        active_count++;
                        RightClickContextMenuStrip.AddGroup(regGroup, IconResolverHandler);
                    }
                }
        }
    }
    static class DefaultToolStripMenus
    {
        public static void AddExitAppMenuItem(this ToolStrip ext, RegSettings2 Checker, bool force=false)
        {
            if (!force)
            {
                force = Checker.GetExitAppMenuFlag();
            }

            if (force)
            {
                ToolStripMenuItem exitAppMenuItem = new ToolStripMenuItem();
                exitAppMenuItem.Text = "Close this app.";
                exitAppMenuItem.Click +=  (sender, e) =>
                {
                    Application.Exit();
                    DebugStuff.WriteLog("User requested to exit the application.");
                    Environment.Exit(0);
                };
                ext.Items.Add(exitAppMenuItem); 
            }
            
        }

        public static void AddRefreshAppMenuItem(this ToolStrip ext, RegSettings2 Checker, MainFormLaunchPad refresh_this_form, bool force=false) 
        {

            if (!force)
            {
                 force = Checker.GetAddRefreshMenuFlag();
            }


            if (force)
            {
                ToolStripMenuItem RefreshMenu = new ToolStripMenuItem();
                RefreshMenu.Text = "Refresh Menu.";
                RefreshMenu.Tag = refresh_this_form;
                RefreshMenu.Click += (sender, e) =>
                {
                    ToolStripMenuItem self = sender as ToolStripMenuItem;
                    MainFormLaunchPad that = (MainFormLaunchPad) self.Tag;
                    if (that != null)
                    {
                        that.RefreshContextMenu("User requested refresh.");
                        that.RefreshContextMenu();

                    }
                };
                ext.Items.Add(RefreshMenu);
            }
        }

        public static void AddSeperator(this ToolStrip ext)
        {
            ToolStripSeparator toolStripSeparator = new ToolStripSeparator();
            ext.Items.Add(toolStripSeparator);
        }
    }
    /// <summary>
    /// Extends the normal tool strip item we stash group menus into letting us do it with a click
    /// </summary>
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
                    try
                    {
                        NewItem.Text = linkObject?.Description;
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        NewItem.Text = Item.Name;
                    }
                    
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
                ToolStrip = new ToolStripMenuItem();
                AddGroupToMenu(ToolStrip, Group, Resolver);
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
}
