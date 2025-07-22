using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CorpTrayLauncher.Shortcuts
{
    /// <summary>
    /// Helper class to resolve a shortcut
    /// </summary>
    static class LinkDispatch
    {
        /// <summary>
        /// Follow link represented by a shell itme
        /// </summary>
        /// <param name="obj"></param>
        public static void FollowLink(ShellLinkObject obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
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
}
