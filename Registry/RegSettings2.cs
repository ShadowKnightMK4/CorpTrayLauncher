using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CorpTrayLauncher.RegistryHandling
{
    
    public interface IRegProvider
    {
        RegistryKey OpenRegKey(string subKey, bool Writable);
        RegistryKey OpenRegKey(RegistryKey Target, string subkey, bool Writable);
        RegistryKey CreateSubKey(RegistryKey Target, string subkey, bool Writable = true);

        object GetValue(RegistryKey Target, string subkey, string valueName, object defaultValue = null);
    }

    class DefaultRegProvider: IRegProvider
    {
        public RegistryKey CreateSubKey(RegistryKey Target, string subkey, bool Writable = true)
        {
            return Target.CreateSubKey(subkey, Writable ? RegistryKeyPermissionCheck.ReadWriteSubTree : RegistryKeyPermissionCheck.ReadSubTree)
                   ?? throw new InvalidOperationException($"Failed to create or open registry key: {subkey} under {Target.Name} with writable set to {Writable}");
        }

        public object GetValue(RegistryKey Target, string subkey, string valueName, object defaultValue = null)
        {
            RegistryKey TargetKey = null;
            try
            {
                if (subkey != null)
                {
                    TargetKey = OpenRegKey(Target, subkey, false);
                }
                else
                {
                    TargetKey = Target;
                }

                {
                  var val = TargetKey.GetValue(valueName, defaultValue);
                    if (val != null)
                        return val;
                    return defaultValue;
                }
            }
            finally
            {

                if ((Target != TargetKey) && ((Target != Registry.CurrentUser) && (Target != Registry.LocalMachine)))
                {
                    Target?.Dispose();
                }
            }
        }

        public RegistryKey OpenRegKey(string subKey, bool Writable)
        {
            return OpenRegKey(Registry.CurrentUser, subKey, Writable);
        }
        public RegistryKey OpenRegKey(RegistryKey Target, string subkey, bool Writable)
        {
            if (Target == null) throw new ArgumentNullException(nameof(Target), "Target registry key cannot be null");
            if (Writable) return Target.OpenSubKey(subkey, Writable) ?? Target.CreateSubKey(subkey);
            return Target.OpenSubKey(subkey, Writable);
            
        }
    }

    public static class RegConfigLocations
    {

       
        public const string PolicySettingLocation = "SOFTWARE\\Policies\\CorpTrayLauncher";
        public const string UserSettingLocation = "Software\\CorpTrayLauncher\\";

        public const string UserTrayIconLocation = "Software\\CorpTrayLauncher\\TrayIcon";
        public const string PolicyTrayIconLocation = "Software\\Policies\\CorpTrayLauncher\\TrayIcon";

        public const string UserGroupLocation = "Software\\CorpTrayLauncher\\Groups";
        /// <summary>
        /// Name of registry key where the folder paths are stored.
        /// </summary>
        public const string FolderRegValueName = "FolderBuildingPath";
        public const string DisabledRegValueName = "Disabled";
        public const string IsTopLevelRegValueName = "IsTopLevel"; // This is used to determine if the group is a top-level group or a sub-group.
        public const string TrayIcon = "TrayIcon"; // This is used to store the icon for the group.
        public const string ToolTipName = "ToolTip"; // This is used to store the tooltip for the tray icon. If not found, generic name is set. Empty means none displayed
        public const string RefreshMenuButton = "RefreshMenuButton"; // used to add a rescan menu groups
        public const string ExitAppMenuButton = "ExitAppMenuButton"; // used to add a quick button to the menu


        public const string ExpandVarsPolicy = "ExpandEnvironmentVariables"; // This is used to determine if environment variables should be expanded in policy settings.
    }

    /// <summary>
    /// Implements enumerating groups and settings defined in both policy and user. TODO: cleave this into half, do internal class and then go public with a wrapper that uses the internal class to do the work
    /// </summary>
    public class RegSettings2
    {
        IRegProvider _regProvider;
        public RegSettings2(IRegProvider regProvider)
        {
            _regProvider = regProvider ?? throw new ArgumentNullException(nameof(regProvider), "Registry provider cannot be null");
        }

        public RegSettings2() : this(new DefaultRegProvider())
        {
        }

        /// <summary>
        /// Is the flag set to add a refresh the menu items?
        /// </summary>
        /// <returns></returns>
        public bool GetAddRefreshMenuFlag()
        {
            bool output = false;
            FetchBoolName(RegConfigLocations.RefreshMenuButton, out output);
            return output;
        }

        /// <summary>
        /// If the flag set to add an exit the app menu item
        /// </summary>
        /// <returns></returns>
        public bool GetExitAppMenuFlag()
        {
            bool output = false;
            FetchBoolName(RegConfigLocations.ExitAppMenuButton, out output);
            return output;
        }

        /// <summary>
        /// Is the flag set to expand enviroment variables in the policy settings?
        /// </summary>
        /// <returns></returns>
        public bool GetExpandEnvironmentVariablesFlag()
        {
            bool output = false;
            FetchBoolName(RegConfigLocations.ExpandVarsPolicy, out output, true);
            return output;
        }

        /// <summary>
        /// Get the folder provider resolver based on the policy setting for expanding environment variables.
        /// </summary>
        /// <returns></returns>
        public IFolderProviderResolver GetFolderProvider()
        {
            if (FetchBoolName(RegConfigLocations.ExpandVarsPolicy, out bool expandVars, true))
            {
                if (expandVars)
                {
                    return new ExpandFolderResolver();
                }
                else
                {
                    return new PassThruFolderResolver();
                }
            }
            else
            {
                return new PassThruFolderResolver();

            }

        }
        /// <summary>
        /// A few common setting check a bool either in policy or user. 
        /// </summary>
        /// <param name="name">name to check</param>
        /// <param name="res">setting value (anything 0 evals to false, non zero to true)</param>
        /// <param name="DoNotCheckUser">disable checking user possible policy</param>
        /// <returns></returns>
        internal bool FetchBoolName(string name, out bool res, bool DoNotCheckUser=false)
        {
            /*
            * Our policy location is HKLM\SOFTWARE\Policies\CorpTrayLauncher\TrayIcon
            * if it exists and accessible, we use that.
            * If it exist not, we check HKCU\Software\CorpTrayLauncher\TrayIcon
            * 
            * if it exists and accessible, we use that.
            * 
            * other wise null
            */

            try
            {
                {
                    using (RegistryKey ItemSetting = _regProvider.OpenRegKey(Registry.LocalMachine, RegConfigLocations.PolicySettingLocation, false))
                    {
                        if ( (ItemSetting != null) &&
                            (ItemSetting.GetValueKind(name) == RegistryValueKind.DWord))
                        {
                            int r = (int)ItemSetting.GetValue(name);
                            if (r != 0)
                                res = true;
                            else
                                res = false;
                            return true;
                        }
                    }
                }
            }
            catch (IOException)
            {
                // just move on
            }
            catch (AccessViolationException)
            {
                // same
            }
            catch (UnauthorizedAccessException)
            {
                // yep
            }




            if (!DoNotCheckUser)
            {

                try
                {
                    using (RegistryKey ItemSetting = _regProvider.OpenRegKey(Registry.CurrentUser, RegConfigLocations.UserSettingLocation, false))
                    {
                        if ((ItemSetting != null) &&
                            (ItemSetting.GetValueKind(name) == RegistryValueKind.DWord))
                        {
                            int r = (int)ItemSetting.GetValue(name);
                            if (r != 0)
                                res = true;
                            else
                                res = false;
                            return true;
                        }
                    }
                }
                catch (IOException)
                {
                    // just move on
                }
                catch (AccessViolationException)
                {
                    // same
                }
                catch (UnauthorizedAccessException)
                {
                    // yep
                }
            }
            res = false;
            return false;
        }
        /// <summary>
        /// Fetch a string out of a group
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal string FetchStringName(string name)
        {

            /*
             * Our policy location is HKLM\SOFTWARE\Policies\CorpTrayLauncher\TrayIcon
             * if it exists and accessible, we use that.
             * If it exist not, we check HKCU\Software\CorpTrayLauncher\TrayIcon
             * 
             * if it exists and accessible, we use that.
             * 
             * other wise null
             */
            
                try
                {
                    {
                        using (RegistryKey TraySetting = _regProvider.OpenRegKey(Registry.LocalMachine, RegConfigLocations.PolicySettingLocation, false))
                        {
                            if (TraySetting != null)
                            {
                                var s = TraySetting.GetValue(name)?.ToString();
                                if (s != null)
                                {
                                    return s; // found in policy
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    // just move on
                }
                catch (AccessViolationException)
                {
                    // same
                }
                catch (UnauthorizedAccessException)
                {
                    // yep
                }
            



            
                try
                {
                    using (RegistryKey TraySetting = _regProvider.OpenRegKey(Registry.CurrentUser, RegConfigLocations.UserSettingLocation, false))
                    {
                        if (TraySetting != null)
                        {
                            var s = TraySetting.GetValue(name);
                            return s.ToString();
                        }
                    }
                }
                catch (IOException)
                {
                    // just move on
                }
                catch (AccessViolationException)
                {
                    // same
                }
                catch (UnauthorizedAccessException)
                {
                    // yep
                }

            
            return null; // No settings found
        }

        /// <summary>
        /// With the tool tip size going "HEY" if larger then 64, this code will default to drop the string to be less than uppper_max which defautslts to 63
        /// </summary>
        /// <param name="upper_max"></param>
        /// <returns></returns>
        public string GetToolTip(int upper_max=63)
        {
            var r= FetchStringName(RegConfigLocations.ToolTipName);
            if (r != null)
            {
                if (r.Length > upper_max)
                    r = r.Substring(0, upper_max);
            }
            else
            {
                r = "Right Click to open";
            }
                return r;
        }
        /// <summary>
        /// Grab the custom tray icon resource string by checking policy location first then user
        /// </summary>
        /// <returns>string to a reasource to link (think lnk style) or null if none found</returns>
        public string GetTrayIconLocation()
        {
            return FetchStringName(RegConfigLocations.TrayIcon);

        }

        /// <summary>
        /// Defines the group type.  Policy groups overshadow user groups of the same name, so if a group exists in both user and policy, it is a policy group we read from
        /// </summary>
        public enum GroupType
        {
            /// <summary>
            /// No group exists, used for values when return means group not found
            /// </summary>
            None,
            /// <summary>
            /// User defined group, superseeded by policy
            /// </summary>
            User,
            /// <summary>
            /// Policy defined group. Should not be modified by the user and superseeds User group of same name
            /// </summary>
            Policy
        }

        /// <summary>
        ///  test if the group is defined in the registry under the given target.
        /// </summary>
        /// <param name="Target">should be <see cref="Registry.CurrentUser"/> or <see cref="Registry.LocalMachine"/> </param>
        /// <param name="name"></param>
        /// <returns></returns>

        internal bool CheckGroup(RegistryKey Target, string name)
        {
            using (RegistryKey key = _regProvider.OpenRegKey(Target, RegConfigLocations.UserGroupLocation, false))
            {
                if (key != null)
                {
                    if (key.SubKeyCount > 0)
                    {
                        // Check if the specific group exists in the user registry
                      return   key.GetSubKeyNames().Contains(name);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Return what type if any group is defined in the registry under the given name. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>If group is both user and policy group and policy group is accessible, that one will win</remarks>
        public GroupType GetGroupType(string name)
        {
            GroupType ret = GroupType.None;
            // check User group first.
            if (CheckGroup(Registry.LocalMachine, name))
            {
                ret = GroupType.Policy;
            }
            // check Policy group next
            else if (CheckGroup(Registry.CurrentUser, name))
            {
                ret = GroupType.User;
            }
            return ret;
        }
        /// <summary>
        /// Does this group exist at all?
        /// </summary>
        /// <param name="name">name of the group to check</param>
        /// <returns>true if yes and false if no</returns>
        /// <remarks>equivolent to testing <see cref="GetGroupType(string)"/> returns something other than <see cref="GroupType.None"/></remarks>
        public bool GroupExists(string name)
        {
            return GetGroupType(name) != GroupType.None;
        }

        /// <summary>
        /// Return a list of groups (aka registry keys) in the target. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ExcludeInactive">by default we skip inactive</param>
        /// <returns></returns>
        /// <remarks>common code for <see cref="GetPolicyGroupNames(bool)"/> and <see cref="GetUserGroupNames(bool)"/></remarks>
        internal List<string> RipGroupName(RegistryKey source, bool ExcludeInactive)
        {
            using (RegistryKey key = _regProvider.OpenRegKey(source, RegConfigLocations.UserGroupLocation, false))
            {
                List<string> groups = new List<string>();
                if (key != null)
                {
                    foreach (string groupName in key.GetSubKeyNames())
                    {
                        if (ExcludeInactive && !IsGroupActive(groupName))
                        {
                            continue; // Skip inactive groups if ExcludeInactive is true
                        }
                        {
                            groups.Add(groupName);
                        }
                    }
                    return groups;
                }
            }
            return null;
        }
        /// <summary>
        /// Get list of all user groups we can find in the registry.
        /// </summary>
        /// <param name="ExcludeInactive">default true, set to true to skip disabled groups</param>
        /// <returns></returns>
        public List<string> GetUserGroupNames(bool ExcludeInactive=true)
        {
            return RipGroupName(Registry.CurrentUser, ExcludeInactive);
        }

        /// <summary>
        /// Get list of all policy groups we can find in the registry.
        /// </summary>
        /// <param name="ExcludeInactive">default true, set to true to skip disabled groups</param>
        /// <returns></returns>
        public List<string> GetPolicyGroupNames(bool ExcludeInactive = true)
        {
            return RipGroupName(Registry.LocalMachine, ExcludeInactive);
        }

        /// <summary>
        /// get a list of all groups on the system, both user and policy groups.
        /// </summary>
        /// <param name="ExcludeInactive"></param>
        /// <returns></returns>
        public List<string> GetGroupNames(bool ExcludeInactive=true)
        {
            List<string> userGroups;
            List<string> policyGroups;
            try
            {
                userGroups = GetUserGroupNames(ExcludeInactive);
            }
            catch (AccessViolationException)
            {
                userGroups = new List<string>();
                DebugStuff.WriteLog("Access violation while fetching user groups. This may indicate a permissions issue.");
            }
            catch (Exception ex)
            {
                userGroups = new List<string>();
                DebugStuff.WriteLog("Error fetching user groups: " + ex.Message);
            }

            try
            {
                policyGroups = GetPolicyGroupNames(ExcludeInactive);
            }
            catch (AccessViolationException)
            {
                policyGroups = new List<string>();
                DebugStuff.WriteLog("Access violation while fetching policy groups. This may indicate a permissions issue.");
            }
            catch (Exception ex)
            {
                policyGroups = new List<string>();
                DebugStuff.WriteLog("Error fetching policy groups: " + ex.Message);
            }
            var allGroups = new List<string>();
            if (userGroups != null)
            {
                allGroups.AddRange(userGroups);
            }
            if (policyGroups != null)
            {
                allGroups.AddRange(policyGroups);
            }
            return allGroups.Distinct().ToList(); // Combine and remove duplicates
        }

        /// <summary>
        /// Internla code for some of the IsGoupActiveStuff
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="Branch"></param>
        /// <param name="T">if set to none, will check BOTH!</param>
        /// <returns></returns>
        internal bool GroupActiveCheck(string groupName, RegistryKey Branch, string location)
        {
            var type = this.GetGroupType(groupName); // Ensure the group exists before checking its status

            RegistryKey target;
            target = Branch;
            /*
            if (type == GroupType.Policy)
            {
                target = Registry.LocalMachine; // Policy groups are in HKLM
                location = RegConfigLocations.PolicySettingLocation;
            }
            else
            {
                target = Registry.CurrentUser; // User groups are in HKCU
                location = RegConfigLocations.UserSettingLocation;
            }*/


            // future me: this check is here rather than top. Why ? incase more group types added
            if (type == GroupType.None)
            {
                return false; // Group does not exist
            }
            using (RegistryKey key = _regProvider.OpenRegKey(target, location + "\\" + groupName, false))
            {
                if (key != null)
                {
                    object disabledValue = key.GetValue(RegConfigLocations.DisabledRegValueName);
                    if (disabledValue != null && disabledValue is int disabledInt)
                    {
                        return disabledInt == 0; // 0 means active, 1 means inactive
                    }
                }
            }
            return true; // If no value is set, assume active
        }
        public bool IsUserGroupActive(string groupname)
        {
            return GroupActiveCheck(groupname, Registry.CurrentUser, RegConfigLocations.UserGroupLocation);
        }

        public bool IsPolicyGroupActive(string groupname)
        {
            return GroupActiveCheck(groupname, Registry.LocalMachine, RegConfigLocations.PolicySettingLocation);
        }
        /// <summary>
        /// The group is active in either user or policy registry?
        /// </summary>
        /// <param name="groupName">name of the group to check</param>
        /// <returns></returns>
        /// <remarks>This means the group's reg key does not have entry <see cref="RegConfigLocations.DisabledRegValueName"/> set to true</remarks>
        public bool IsGroupActive(string groupName)
        {
            return IsPolicyGroupActive(groupName) || IsUserGroupActive(groupName);
        }
      


           
    
    }
}
