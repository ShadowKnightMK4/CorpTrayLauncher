using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace CorpTrayLauncher
{
    public static class DebugStuff
    {
        public static TextBox LogWindows = null;

        public static void WriteLog(string message)
        {
            if (!Debugger.IsAttached)
            {
                // If not in debug mode, do not log
                return;
            }
            if (LogWindows != null)
            {
                LogWindows.AppendText(message + Environment.NewLine + Environment.NewLine);
                LogWindows.SelectionStart = LogWindows.Text.Length;
                LogWindows.ScrollToCaret();
            }
            else
            {
                Console.WriteLine(message);
            }
            Debugger.Log(Debugger.IsLogging() ? 0 : 1, "Debug", message + Environment.NewLine + Environment.NewLine);

        }

    }
}
