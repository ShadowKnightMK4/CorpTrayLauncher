using Shell32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace CorpTrayLauncher.IconHandling
{

    /// <summary>
    /// Private the way to  ideally get an icon as image from a file system idem, optional icon index. Dispoabling inherit is future. Scope is caching icons that are from same places ect.. and returning thoses instead than load icon for each image repeated. Id if a game and and editor have the same icon, we get one floating in our memory
    /// </summary>
    public interface IExtractIconProvider : IDisposable
    {
        /// <summary>
        /// This should when given a file item and an optional icon index, return the icon as an Image.
        /// </summary>
        /// <param name="filePath">file item to source icon from</param>
        /// <param name="iconIndex">index in the item to source</param>
        /// <returns></returns>
        Image GetIconAsImage(string filePath, int iconIndex = 0);

        Icon GetIconAsIcon(string filePath, int iconIndex = 0);
    }

    public interface IResolveIconFromPath : IDisposable
    {         /// <summary>
              /// Resolves the icon for a given file path.
              /// </summary>
              /// <param name="filePath">The file path to resolve the icon from.</param>
              /// <param name="iconIndex">The index of the icon to retrieve.</param>
              /// <returns>An Image representing the icon, or null if not found.</returns>
        Image ResolveIcon(string icon_file_item, int Index = 0);
        /// <summary>
        /// Resolves the icon for a given ShellLinkObject. If none is defined, default to Icon of the target. Should that be blank, the menu item won't have an icon.
        /// </summary>
        /// <param name="linkObject"></param>
        /// <returns>Image of the icon or null</returns>
        Image ResolveIcon(ShellLinkObject linkObject);
    }

}
