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
using System.Diagnostics;
namespace CorpTrayLauncher
{

    static class RegGroupDispatcher
    {
        /// <summary>
        /// Add the groups to the context menu passed. This is functionally the heart of the generator
        /// </summary>
        /// <param name="Groups">list of groups to add. While we assume they're active, no such check is here as this is called by other code that does the checking</param>
        /// <param name="IconResolverHandler">can be null, this code will resolve icon requests from shortcuts, ect... as needed.</param>
        /// <param name="FolderResolver">Should NOT be null. This translates a folder string file in in the <see cref="RegConfigLocations.FolderRegValueName"/> group key into real live folders. Used to expand env variables</param>
        /// <param name="RightClickContextMenuStrip">The menu strip to add too</param>
        /// <param name="active_count"></param>
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

            if (FolderResolver == null)
            {
                string msg = "Warning/Fatal error: AddGroup() must have a IFolderProviderResolver class passed to it.";
                DebugStuff.WriteLog(msg);
                if (Debugger.IsAttached)
                    Debugger.Break();
                else
                {
                    MessageBox.Show("Warning/Fatal error: AddGroup() must have a IFolderProviderResolver class passed to it. This is is a logic error");
                    Application.Exit();
                    Environment.Exit(-999999);
                    return;
                }
            }

            
            // if no groups to add, nothing to do, log it and go home

            if ( (Groups == null) || (Groups.Count == 0))
            {
                DebugStuff.WriteLog("No groups added cause empty Group or null group passed to RegGroupDispatcher.");
                return;
            }
            // loop thru the groups passed, passing the name, icon handler and folder handler to WritableRegGroup
                for (int i = 0; i < Groups.Count; i++)
                {
                    var group = Groups[i];
                    if (!string.IsNullOrWhiteSpace(group))
                    {
                        RegGroup regGroup = new WritableRegGroup(group, IconResolverHandler, FolderResolver);
                        if (!regGroup.IsEnabled)
                        {
                            // log we're skipping 
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
                        // stat track cause why note
                        active_count++;

                        // this routine here finishes up and adds the group to the passed context menu
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
                // create the exit tool strip item, include the code to exit the app 
                ToolStripMenuItem exitAppMenuItem = new ToolStripMenuItem();
                exitAppMenuItem.Text = "Close this app.";
                exitAppMenuItem.Click +=  (sender, e) =>
                {
                    Application.Exit();
                    DebugStuff.WriteLog("User requested to exit the application.");
                    Environment.Exit(0);
                    return;
                };
                // hook it up
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
                // create the refresh menu item, assign the event code to call the refresh and go
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
                        //that.RefreshContextMenu();

                    }
                };
                // add it too
                ext.Items.Add(RefreshMenu);
            }
        }

        /// <summary>
        /// Nothing fancy, just add a seperator to this toolstrip
        /// </summary>
        /// <param name="ext">the ToolStrip to add too</param>
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

        /// <summary>
        /// Another holder routine. This will grab Shortcuts (.LNK) files from the directory, make a menu item for them and add to either OtherTarget or Target if any of them not null
        /// </summary>
        /// <param name="Source">Directory to scan for .LNK files</param>
        /// <param name="Target">Target to add menu for.  For top level menu, this is not null.</param>
        /// <param name="OtherTarget">Target to add menu for. For a NON top level menu, this is not null</param>
        /// <param name="IconHandler">can techinically be null, but we need non null for icons</param>
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
            // loop thru the found items
            foreach (var Item in Items)
            {
                FolderItem shellitem = Help.NameSpace(Source.FullName).ParseName(Item.Name);
                if (shellitem.IsLink)
                {
                    // yay a shortcut, create a tool strip thing for it, attempt to extract what we need and assign the new toolstrip.tag as our ShellLinkObject
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

        /// <summary>
        /// Add a group to a submenu of the context menu, assuming Ext is a child of a context menu
        /// </summary>
        /// <param name="Ext">The direct menu to add the processed group too</param>
        /// <param name="Group">Group to add</param>
        /// <param name="IconHandler">can techinically be null, but we need non null for icons</param>
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

        /// <summary>
        /// Add a top level group to our main context menu
        /// </summary>
        /// <param name="Ext">context menu to add too</param>
        /// <param name="Group"></param>
        /// <param name="Iconhandler"></param>
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
        /// <param name="Ext">The context menu we add too</param>
        /// <param name="Group">The Group we are added</param>
        /// <param name="Resolver">If we want icons added too, this should be not null</param>
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
