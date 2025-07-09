using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CorpTrayLauncher
{
    public partial class MainFormLaunchPad : Form
    {
        public MainFormLaunchPad()
        {
            InitializeComponent();
        }

        TextBox TextBoxDebug = null;
        private void NotifiyIconTaskBarHandler_MouseClickHandler(object sender, MouseEventArgs e)
        {
            RightClickContextMenuStrip.Show(Cursor.Position);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                this.Hide();
            }
            else
            {
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

            NotifiyIconTaskBarHandler.Visible = true;
            NotifiyIconTaskBarHandler.Icon = SystemIcons.Application;
            NotifiyIconTaskBarHandler.MouseClick += NotifiyIconTaskBarHandler_MouseClickHandler;
            NotifiyIconTaskBarHandler.ContextMenuStrip = RightClickContextMenuStrip;



            int active_count = 0;
            // grab all the groups stored in the reg that ARE active
            var Groups = RegSettingChecker.GetGroups(null);
            DebugStuff.WriteLog("Found " + Groups.Count + " groups in the registry.");
            for (int i = 0; i < Groups.Count; i++)
            {
                var group = Groups[i];
                if (group.IsEnabled)
                {
                    active_count++;
                    RightClickContextMenuStrip.AddGroup(Groups[i]);
                }
            }
            DebugStuff.WriteLog("Added " + active_count + " groups marked active in the registry to the menu");
        }
    }
}
