using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CorpTrayLauncher.IconHandling;
using CorpTrayLauncher.RegistryHandling;

namespace CorpTrayLauncher
{

    public partial class MainFormLaunchPad : Form
    {
        public MainFormLaunchPad()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Outside of Debugger attached, this is always null, inside debugger attached, serves as a running log of what's happened
        /// </summary>

        TextBox TextBoxDebug = null;
        /// <summary>
        /// <see cref="RegGroup"/> and kin will use this one load icons
        /// </summary>
        IconResolverHandler IconResolverHandler = new IconResolverHandler();
        /// <summary>
        /// Our registry we use. Running code uses the default provider, unit tests not so much
        /// </summary>
        RegSettings2 RegSettings = new RegSettings2();
        bool DisposeIconOnClose = false;
        private void NotifiyIconTaskBarHandler_MouseClickHandler(object sender, MouseEventArgs e)
        {
            // right click to bring the menu up
            if (e.Button == MouseButtons.Right)
                RightClickContextMenuStrip.Show(Cursor.Position);
        }


        /// <summary>
        /// Serves as easy way to refresh the menu, logs in debug window
        /// </summary>
        /// <param name="why"></param>
        public void RefreshContextMenu(string why)
        {
            DebugStuff.WriteLog(why + "\r\n\r\n");
            RefreshContextMenu();
        }

        /// <summary>
        /// Refresh our context menu
        /// </summary>
        /// <remarks>no groups found will trigger an exit</remarks>
        public void RefreshContextMenu()
        {
            /* these two bools hold if the refresh and exit menu are wanted in the registry setting*/
            bool refresh;
            bool exit;

            int active_count = 0;
            // grab all the groups stored in the reg that ARE active
            var Groups = RegSettings.GetGroupNames(false);

            if (Groups == null || Groups.Count == 0)
            {
                DebugStuff.WriteLog("No groups found in the registry, exiting.");
                Application.Exit();
                Environment.Exit(0);
                return;
            }
            else
            {
                DebugStuff.WriteLog("Found " + Groups.Count + " groups in the registry.");
            }

            // assign that menu tag to ourself to let it grab if needed
            this.RightClickContextMenuStrip.Tag = this;

            // cleanup the old menu if any and clear disposed items
            for (int i = 0; i < RightClickContextMenuStrip.Items.Count; i++)
            {
                RightClickContextMenuStrip.Items[i]?.Dispose();
            }
            RightClickContextMenuStrip.Items.Clear();

            // this routine called here adds the groups we've just loaded if they be active 
            RegGroupDispatcher.AddGroups(Groups, IconResolverHandler, RegSettings.GetFolderProvider(),  RightClickContextMenuStrip,  out active_count);
            DebugStuff.WriteLog("Added " + active_count + " groups marked active in the registry to the menu");

            
            // grab our flags
            refresh = RegSettings.GetAddRefreshMenuFlag();
            exit = RegSettings.GetExitAppMenuFlag();

            if (Debugger.IsAttached)
            {
                // in debug mode, always add the refresh and exit menu items
                DebugStuff.WriteLog("If Debugger attached: Default Menu Items always added ( Refresh and Exit menu items)\r\n");
                DebugStuff.WriteLog($"Settings read for refresh: {refresh}. Setting read for exit {exit}.");
                refresh = true;
                exit = true;
            }
            DebugStuff.WriteLog($"Current setting to expand ENV vars: {RegSettings.GetExpandEnvironmentVariablesFlag()}.\r\n");


            if (refresh || exit)
            {
                DefaultToolStripMenus.AddSeperator(RightClickContextMenuStrip);
                if (refresh)
                {
                    DefaultToolStripMenus.AddRefreshAppMenuItem(RightClickContextMenuStrip, RegSettings, this, true);
                }
                if (exit)
                {
                    DefaultToolStripMenus.AddExitAppMenuItem(RightClickContextMenuStrip, RegSettings, true);
                }
            }
            
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {

            if (Debugger.IsAttached == false)
            {
                this.Hide();
                this.Visible = false;
                this.ShowInTaskbar = false;
            }
            else
            {
                // add the debug log
                this.Show();
                this.Text = "Debug Mode: " + Assembly.GetExecutingAssembly().GetName().Name;
                TextBoxDebug = new TextBox
                {
                    Multiline = true,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Both,
                    Font = new Font("Consolas", 10)
                };
                this.Controls.Add(TextBoxDebug);
                TextBoxDebug.Text = "Receives log output of actions done, switches tripped, ect...\r\n";
                DebugStuff.LogWindows = TextBoxDebug; // set the debug log window to this textbox
                DebugStuff.WriteLog("Debug Mode: " + Assembly.GetExecutingAssembly().GetName().Name + "Online");
                DebugStuff.WriteLog("CorpTrayLauncher started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                DebugStuff.WriteLog("CorpTrayLauncher version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                DebugStuff.WriteLog("CorpTrayLauncher assembly: " + Assembly.GetExecutingAssembly().GetName().Name);

                
                
            }

            // place the icon in the taskbar and add our context menu
            NotifiyIconTaskBarHandler.Visible = true;
            NotifiyIconTaskBarHandler.MouseClick += NotifiyIconTaskBarHandler_MouseClickHandler;
            NotifiyIconTaskBarHandler.ContextMenuStrip = RightClickContextMenuStrip;
            NotifiyIconTaskBarHandler.Text = RegSettings.GetToolTip();

            // set our icon if redefined in the registry and loadable.
            string TrayIcon = RegSettings.GetTrayIconLocation();

            if (string.IsNullOrEmpty(TrayIcon))
            {
                // no icon defined 
                NotifiyIconTaskBarHandler.Icon = SystemIcons.Application;
            }
            else
            {
                // if not loadabl
                NotifiyIconTaskBarHandler.Icon = this.IconResolverHandler.ExtractIconViaProvider(TrayIcon, 0);
                DisposeIconOnClose = true;
            }
            // run the first scan of of the groups to populate the menu
            RefreshContextMenu();
        }

        
        private void MainFormLaunchPad_FormClosed(object sender, FormClosedEventArgs e)
        {
            DebugStuff.WriteLog("Exit app done by form close.");
            Application.Exit();
            Environment.Exit(0);
            if (NotifiyIconTaskBarHandler != null)
            {
                if (NotifiyIconTaskBarHandler.Visible)
                {
                    NotifiyIconTaskBarHandler.Visible = false;
                }
                if (DisposeIconOnClose && NotifiyIconTaskBarHandler.Icon != null)
                {
                    NotifiyIconTaskBarHandler.Icon.Dispose();
                    NotifiyIconTaskBarHandler.Icon = null;
                }
                NotifiyIconTaskBarHandler.Dispose();
                NotifiyIconTaskBarHandler = null;
            }
        }

        private void MainFormLaunchPad_FormClosing(object sender, FormClosingEventArgs e)
        {
 
            DebugStuff.WriteLog("Form closing. prepping to exit\r\n");
            if (IconResolverHandler != null)
            {
                IconResolverHandler.Dispose();
            }

        }
    }
}
