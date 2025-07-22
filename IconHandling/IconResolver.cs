using Shell32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace CorpTrayLauncher.IconHandling
{
    /// <summary>
    /// The default iplementation of <see cref="IExtractIconProvider"/> just wraps the Windows API to extract an icon from a file.
    /// </summary>
    internal class DefaultIconExtractImport: IExtractIconProvider
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern uint ExtractIconEx(string szFileName, int nIconIndex,
   IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);
        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr hIcon);

        public virtual void Dispose()
        {
            ; // none needed
        }

        public Image GetIconAsImage(string filePath, int iconIndex = 0)
        {
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];
            uint iconCount = ExtractIconEx(filePath, iconIndex, largeIcons, smallIcons, 1);
            if (iconCount > 0 && largeIcons[0] != IntPtr.Zero)
            {
                Image iconImage = Icon.FromHandle(largeIcons[0]).ToBitmap();
                DestroyIcon(largeIcons[0]);
                return iconImage;
            }
            return null;
        }

        public Icon GetIconAsIcon(string filePath, int iconIndex = 0)
        {
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];
            uint iconCount = ExtractIconEx(filePath, iconIndex, largeIcons, smallIcons, 1);
            if (iconCount > 0 && largeIcons[0] != IntPtr.Zero)
            {
                //return Icon.FromHandle(largeIcons[0]); orignal.
                {
                 return  (Icon) (Icon.FromHandle(largeIcons[0])).Clone();
                }
            }
            return null;
        }
    }
    /// <summary>
    /// Fetch icon for a ShellLinkObject, or the target if no icon is defined.
    /// </summary>
    public class IconResolverHandler: IResolveIconFromPath
    {
        readonly IExtractIconProvider ExtractIconAPI;

        public Icon ExtractIconViaProvider(string source, int iconIndex = 0)
        {
            if (ExtractIconAPI == null)
            {
                throw new InvalidOperationException("ExtractIconProvider is not initialized.");
            }
            return ExtractIconAPI.GetIconAsIcon(source, iconIndex);
        }

        public Image ExtractIconAsImage(string source, int iconIndex = 0)
        {
            if (ExtractIconAPI == null)
            {
                throw new InvalidOperationException("ExtractIconProvider is not initialized.");
            }
            return ExtractIconAPI.GetIconAsImage(source, iconIndex);
        }


        public IconResolverHandler(IExtractIconProvider ExtractIconHandler)
        {
            ExtractIconAPI = ExtractIconHandler ?? throw new ArgumentNullException(nameof(ExtractIconHandler), "ExtractIconProvider cannot be null");
        }

        public IconResolverHandler() : this(new DefaultIconExtractImport())
        {
        }

        /// <summary>
        /// Resolves the icon for a given ShellLinkObject. If none is defined, default to Icon of the target. Should that be blank, the menu item won't have an icon.
        /// </summary>
        /// <param name="linkObject"></param>
        /// <returns>null or <see cref="Image>"/> for <see cref="ToolStripItem.Image"/></returns>
        public Image ResolveIcon(ShellLinkObject linkObject)
        {
            if (linkObject == null)
            {
                return null;
            }

            string icon_file_item;
            int icon_index;
            icon_index = linkObject.GetIconLocation( out icon_file_item);

            
            Image x;
            if (string.IsNullOrEmpty(icon_file_item))
            {
                icon_file_item = linkObject.Path;
                DebugStuff.WriteLog("IconResolver: No icon file defined, using Path: " + icon_file_item);
                if (icon_file_item == null || icon_file_item.Length == 0)
                {
                    // no icon defined, return null
                    icon_file_item = linkObject.Target.Name;
                    DebugStuff.WriteLog("IconResolver: No icon file defined, using Target.Name: " + icon_file_item);
                }
            }

            
            return ExtractIconAPI.GetIconAsImage(icon_file_item, icon_index);
        }

        public Image ResolveIcon(string icon_file_item, int Index = 0)
        {
            return ExtractIconAPI.GetIconAsImage(icon_file_item, Index);
        }

        public void Dispose()
        {
            ExtractIconAPI.Dispose();
        }
    }
}
