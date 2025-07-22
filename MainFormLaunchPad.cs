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

        TextBox TextBoxDebug = null;
        IconResolverHandler IconResolverHandler = new IconResolverHandler();
        RegSettings2 RegSettings = new RegSettings2();
        bool DisposeIconOnClose = false;
        private void NotifiyIconTaskBarHandler_MouseClickHandler(object sender, MouseEventArgs e)
        {
            
            RightClickContextMenuStrip.Show(Cursor.Position);
        }


        public void RefreshContextMenu(string why)
        {
            DebugStuff.WriteLog(why + "\r\n\r\n");
            RefreshContextMenu();
        }
        public void RefreshContextMenu()
        {
            int active_count = 0;
            // grab all the groups stored in the reg that ARE active
            var Groups = RegSettings.GetGroupNames(false);

            if (Groups == null || Groups.Count == 0)
            {
                DebugStuff.WriteLog("No groups found in the registry, exiting.");
                Application.Exit();
            }
            else
            {
                DebugStuff.WriteLog("Found " + Groups.Count + " groups in the registry.");
            }
            this.RightClickContextMenuStrip.Tag = this;
            for (int i = 0; i < RightClickContextMenuStrip.Items.Count; i++)
            {
                RightClickContextMenuStrip.Items[i]?.Dispose();
            }
            RightClickContextMenuStrip.Items.Clear();

            RegGroupDispatcher.AddGroups(Groups, IconResolverHandler, RegSettings.GetFolderProvider(),  RightClickContextMenuStrip,  out active_count);
            DebugStuff.WriteLog("Added " + active_count + " groups marked active in the registry to the menu");

            bool refresh;
            bool exit;


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
            if (System.Diagnostics.Debugger.IsAttached == false)
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

            // place the icon in the taskbar
            NotifiyIconTaskBarHandler.Visible = true;
            
            NotifiyIconTaskBarHandler.MouseClick += NotifiyIconTaskBarHandler_MouseClickHandler;
            NotifiyIconTaskBarHandler.ContextMenuStrip = RightClickContextMenuStrip;
            NotifiyIconTaskBarHandler.Text = RegSettings.GetToolTip();
            string TrayIcon = RegSettings.GetTrayIconLocation();
            if (string.IsNullOrEmpty(TrayIcon))
            {
                NotifiyIconTaskBarHandler.Icon = SystemIcons.Application;
            }
            else
            {
                NotifiyIconTaskBarHandler.Icon = this.IconResolverHandler.ExtractIconViaProvider(TrayIcon, 0);
                DisposeIconOnClose = true;
            }

            RefreshContextMenu();
        }

        
        private void MainFormLaunchPad_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DisposeIconOnClose && NotifiyIconTaskBarHandler.Icon != null)
            {
                NotifiyIconTaskBarHandler.Icon.Dispose();
                NotifiyIconTaskBarHandler.Icon = null;
            }
            DebugStuff.WriteLog( "Form closed");
            if (IconResolverHandler != null)
            {
                IconResolverHandler.Dispose();
            }

            
            DebugStuff.WriteLog("Exit app done by form close.");
            Application.Exit();
            Environment.Exit(0);
        }

        private void MainFormLaunchPad_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
