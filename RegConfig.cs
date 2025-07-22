using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using Shell32;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using CorpTrayLauncher.IconHandling;
using CorpTrayLauncher.Shortcuts;
namespace CorpTrayLauncher.RegistryHandling
{




    
     /// <summary>
    /// Represents a read only view of a reg group
    /// </summary>
    public abstract class RegGroup :IDisposable
    {
        readonly string name;
        readonly IResolveIconFromPath IconHandler;
        readonly IRegProvider Reg;
        readonly IFolderProviderResolver FolderResolver;
        protected RegistryKey GroupKey;
        RegistryKey Source;
        bool _IsWritable = false;
        public  bool IsReadonly => _IsWritable == false; // If writable is false, this is a read only group
        public  RegSettings2.GroupType GetGroupType()
        { 
            if (GroupKey != null)
            {
                if (Source == Registry.LocalMachine)
                {
                    if (GroupKey.Name.Contains(RegConfigLocations.PolicySettingLocation))
                    {
                        return RegSettings2.GroupType.Policy;
                    }
                }
                else if (Source == Registry.CurrentUser)
                {
                    if (GroupKey.Name.Contains(RegConfigLocations.UserGroupLocation))
                    {
                        return RegSettings2.GroupType.User;
                    }
                }
                
            }
            return RegSettings2.GroupType.None;
        }
        /// <summary>
        /// Create a group class to ready this reg key entry, Handler will resolve icons. If blank, an <see cref="IconResolverHandler"/> is used
        /// </summary>
        /// <param name="FromName">open this group</param>
        /// <param name="Handler">handler to resolving icons</param>
        public RegGroup(string FromName, IResolveIconFromPath Handler, IFolderProviderResolver FolderResolver, bool Writable=false)
        {
            this.FolderResolver = FolderResolver ?? throw new ArgumentNullException(nameof(FolderResolver), "Folder resolver cannot be null");
            _IsWritable = Writable; // Set the writable flag based on the parameter
            Reg = new DefaultRegProvider(); // Default registry provider if none provided
            /* our plan 
             * is to first try opening as a policy group. If that's blocked, user group
             * */
            try
            {
                if (string.IsNullOrEmpty(FromName))
                {
                    throw new ArgumentException("Group name cannot be null or empty", nameof(FromName));
                }
            }
            catch (ArgumentException ex)
            {
                DebugStuff.WriteLog("Error creating RegGroup: " + ex.Message);
                throw; // Re-throw the exception to be handled by the caller
            }

            this.Source = Registry.LocalMachine;

            try
            {
                GroupKey = Reg.OpenRegKey(Source, RegConfigLocations.UserGroupLocation + "\\" + FromName, Writable);
            }
            catch (UnauthorizedAccessException e)
            {
                DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                DebugStuff.WriteLog("Skipping trying to load from policy.");
                GroupKey = null;
            }
            catch (Exception e)
            {
                DebugStuff.WriteLog("Error to open registry key: " + e.Message);
                DebugStuff.WriteLog("Skipping trying to load from policy.");
                GroupKey = null;
            }

            if (GroupKey == null)
            {
                Source = Registry.CurrentUser; // Fallback to CurrentUser if LocalMachine fails
                try
                {
                    GroupKey = Reg.OpenRegKey(Source, RegConfigLocations.UserGroupLocation + "\\" + FromName, Writable);
                }
                catch (AccessViolationException e)
                {
                    DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                    throw;
                }
            }


         //GroupKey = Registry.CurrentUser.OpenSubKey(RegConfigLocations.UserGroupLocation + "\\" + FromName);
            if (GroupKey == null)
            {
                if (_IsWritable)
                {
                    GroupKey = Reg.CreateSubKey(Source, RegConfigLocations.UserGroupLocation + "\\" + FromName);
                    if (GroupKey == null)
                    {
                        throw new InvalidOperationException("Failed to or open registry key for group: " + FromName);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Group key does not exist and is not writable: " + FromName);
                }
            }
            name = FromName;
            if (Handler == null)
            {
                IconHandler = new IconResolverHandler(); // Default icon handler if none provided
            }
            else
            {
                IconHandler = Handler; // Use the provided icon handler
            }
        }
        
        public RegGroup(string FromName, IResolveIconFromPath Handler, IFolderProviderResolver FolderResolver, IRegProvider RegistryProvider, bool Writable=false)
        {
            this.FolderResolver = FolderResolver ?? throw new ArgumentNullException(nameof(FolderResolver), "Folder resolver cannot be null");
            this._IsWritable = Writable; // Set the writable flag based on the parameter
            if (RegistryProvider == null)
            {
                throw new ArgumentNullException(nameof(Registry), "Registry provider cannot be null");
            }
            Reg = RegistryProvider;
            try
            {
                if (string.IsNullOrEmpty(FromName))
                {
                    throw new ArgumentException("Group name cannot be null or empty", nameof(FromName));
                }
            }
            catch (ArgumentException ex)
            {
                DebugStuff.WriteLog("Error creating RegGroup: " + ex.Message);
                throw; // Re-throw the exception to be handled by the caller
            }

            try
            {
                GroupKey = Reg.OpenRegKey(Registry.LocalMachine, RegConfigLocations.UserGroupLocation + "\\" + FromName, false);
            }
            catch (AccessViolationException e)
            {
                DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                DebugStuff.WriteLog("Skipping trying to load from policy.");
            }

            if (GroupKey == null)
            {
                try
                {
                    GroupKey = Reg.OpenRegKey(Registry.CurrentUser, RegConfigLocations.UserGroupLocation + "\\" + FromName, false);
                }
                catch (AccessViolationException e)
                {
                    DebugStuff.WriteLog("Access violation while trying to open registry key: " + e.Message);
                    throw;
                }
            }


            //GroupKey = Registry.CurrentUser.OpenSubKey(RegConfigLocations.UserGroupLocation + "\\" + FromName);
            if (GroupKey == null)
            {
                if (_IsWritable)
                {
                    GroupKey = Reg.CreateSubKey(Registry.CurrentUser, RegConfigLocations.UserGroupLocation + "\\" + FromName);
                    if (GroupKey == null)
                    {
                        throw new InvalidOperationException("Failed to create or open registry key for group: " + FromName);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Group key does not exist and is not writable: " + FromName);
                }
            }
            name = FromName;
            if (Handler == null)
            {
                IconHandler = new IconResolverHandler(); // Default icon handler if none provided
            }
            else
            {
                IconHandler = Handler; // Use the provided icon handler
            }
        }


        /// <summary>
        /// Get the name of the group ie registry get we created this class from.
        /// </summary>
        public string Name => name;


        /// <summary>
        /// return if this group of icons are top level or not. Top level groups are the ones that are displayed in the main menu, while sub-groups are displayed in the sub-menus.
        /// </summary>
        public bool IsTopLevel
        {
            get
            {
                if (GroupKey != null)
                {
                    object value = GroupKey.GetValue(RegConfigLocations.IsTopLevelRegValueName);
                    if (value != null && value is int intValue)
                    {
                        return intValue == 1; // 1 means true, 0 means false
                    }
                }
                return false; // Default to false if not set
            }
        }
        /// <summary>
        /// Return if this particular group is enabled or not. On refreshing menu. anything not enabled is skipped
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (GroupKey != null)
                {
                    object value = GroupKey.GetValue(RegConfigLocations.DisabledRegValueName);
                    if (value != null && value is int intValue)
                    {
                        return intValue == 0;
                    }
                }
                return true;
            }
        }

        public string GetGroupIconLocation()
        {
            if (GroupKey != null)
            {
                //object value = GroupKey.GetValue(RegConfigLocations.TrayIcon);
                object value = this.Reg.GetValue(GroupKey, null, RegConfigLocations.TrayIcon, null);
                if (value != null && value is string iconPath && !string.IsNullOrEmpty(iconPath))
                {
                    return value.ToString();
                    //Image icon = IconHandler.ResolveIcon(value.ToString());
                    //return icon;
                }
            }
            return null; // No icon defined, return null
        }

        /// <summary>d
        /// Get the image if any defined for this group. This is used to display the icon in the context menu. Blank is fine.
        /// </summary>
        /// <returns></returns>
        public Image GetGroupIcon()
        {
            string location = GetGroupIconLocation();
            if (!string.IsNullOrEmpty(location))
            {
                try
                {
                    return IconHandler.ResolveIcon(location);
                }
                catch (Exception ex)
                {
                    DebugStuff.WriteLog("Error resolving icon: " + ex.Message);
                    return null; // Return null if there's an error resolving the icon
                }
            }
            return null;
        }



        /// <summary>
        /// Grab all directories out of the folder exist. Should the registry key not exist, it will return the default directories.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Default Directory list is exe running path and a subfolder of that "Itmes"</remarks>
        public List<DirectoryInfo> GetDirectories()
        {
            List<DirectoryInfo> directories = new List<DirectoryInfo>();
            if (GroupKey != null)
            {
                string[] paths = GroupKey.GetValue(RegConfigLocations.FolderRegValueName)?.ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (paths != null && paths.Length > 0)
                {
                    

                    directories = paths.Select(path => new DirectoryInfo(FolderResolver.GetFolder  (path.Trim()))).ToList();
                    // Filter out directories that do not exist
                    directories = directories.Where(dir => dir.Exists).ToList();
                }
                else
                {
                    // If no paths are set, return default paths
                    directories.Add(new DirectoryInfo(Directory.GetCurrentDirectory()));
                    directories.Add(new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Items")));
                }
            }
            else
            {
                // If the group key does not exist, return an empty list
                directories = new List<DirectoryInfo>();
            }
            return directories;
        }

        /// <summary>
        /// Clean up!
        /// </summary>
        public void Dispose()
        {
            if (GroupKey != null)
            {
                GroupKey.Dispose();
                GroupKey = null;
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// A read only view of a group
    /// </summary>
    public class ReadableRegGreg : RegGroup
    {
        public ReadableRegGreg(string FromName, IResolveIconFromPath Handler, IFolderProviderResolver FolderResolver) : base(FromName, Handler, FolderResolver, false)
        {
            
        }

        public ReadableRegGreg(string FromName, IResolveIconFromPath Handler, IFolderProviderResolver FolderResolver, IRegProvider RegistryProvider): base(FromName, Handler, FolderResolver, RegistryProvider, false)
        {
            
        }
    }

    /// <summary>
    /// a read and writable view of the group.
    /// </summary>
    public class WritableRegGroup: RegGroup
    {
        public WritableRegGroup(string FromName, IResolveIconFromPath Handler, IFolderProviderResolver FolderResolver) : base(FromName, Handler, FolderResolver, true)
        {
            Refresh();
        }

        #region OurCache
        bool _IsTopLevel = false;
        bool _IsEnabled = false;
        string _Name = string.Empty;
        public readonly List<string> _Directories = new List<string>();
        string _GroupIcon;

        /// <summary>       
        #endregion
        bool Changed;

        public new List<string> GetDirectories()
        {
            return _Directories;
        }
        public new string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    Changed = true;
                }
            }
        }
        public new bool IsTopLevel
        {
            get { return _IsTopLevel; }
            set
            {
                if (_IsTopLevel != value)
                {
                    _IsTopLevel = value;
                    Changed = true;
                }
            }
        }
        public new bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    Changed = true;
                }
            }
        }

        public string GroupIconLocation
        {
            get { return _GroupIcon; }
            set
            {
                if (_GroupIcon != value)
                {
                    _GroupIcon = value;
                    Changed = true;
                }
            }
        }

        /// <summary>
        /// Pull the stored group data into our class and discard changes
        /// </summary>
        public void Refresh()
        {
            _IsTopLevel = base.IsTopLevel;
            _IsEnabled = base.IsEnabled;
            _Name = base.Name;
            _Directories.Clear();
            foreach (var dir in base.GetDirectories())
            {
                _Directories.Add(dir.FullName);
            }
            _GroupIcon = base.GetGroupIconLocation();
        }

        /// <summary>
        /// Commit the group changes to the regitry.
        /// </summary>
        public void Commit()
        {
            if (IsReadonly)
            {
                throw new InvalidOperationException("Cannot commit changes to a read-only group.");
            }
            var GroupType = GetGroupType();
            if (Changed)
            {
                base.GroupKey.SetValue(RegConfigLocations.IsTopLevelRegValueName, _IsTopLevel, RegistryValueKind.DWord);
                if (_IsEnabled)
                    base.GroupKey.SetValue(RegConfigLocations.DisabledRegValueName, 0,  RegistryValueKind.DWord);
                else
                    base.GroupKey.SetValue(RegConfigLocations.DisabledRegValueName, 1, RegistryValueKind.DWord);
                base.GroupKey.SetValue(RegConfigLocations.FolderRegValueName, string.Join(";", _Directories));
                Changed = false;
                Refresh();
            }
        }

        public void ExportAsJson(Stream Output)
        {
            throw new NotImplementedException();
        }

        public WritableRegGroup CreateFromJson(Stream Input)
        {
            throw new NotImplementedException();
        }



    }


 


}
