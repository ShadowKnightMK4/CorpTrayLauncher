using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorpTrayLauncher.Shortcuts;

namespace CorpTrayLauncher.RegistryHandling
{

    /// <summary>
    /// Translate a folder name to a path. This is used to resolve the folder names stored in the registry to actual paths on the file system and the only implemetnation is used to expand enviorment vars if needed
    /// </summary>
    public interface IFolderProviderResolver
    {
        /// <summary>
        /// If convert folder name to where we want to read from
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        string GetFolder(string FolderName);
    }


    /// <summary>
    /// the folder vars are not expanded. This literaly is a pass thru
    /// </summary>
    public class PassThruFolderResolver : IFolderProviderResolver
    {
        public string GetFolder(string FolderName)
        {
            return FolderName;
        }
    }

    /// <summary>
    /// This version calls expand viroment variables before returning the string
    /// </summary>
    public class ExpandFolderResolver : IFolderProviderResolver
    {
        /// <summary>
        /// The folder vars are expanded
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public string GetFolder(string FolderName)
        {
            return Environment.ExpandEnvironmentVariables(FolderName);
        }
    }
}
