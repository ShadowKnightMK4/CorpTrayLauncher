using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorpTrayLauncher.RegistryHandling;
using CorpTrayLauncher.IconHandling;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Drawing;
namespace GroupEditor
{
    /// <summary>
    /// Varient of RegGroup supporting writing and modifying registry keys.
    /// </summary>
    internal class WritableRegGroup : RegGroup
    {

        #region OurCache
            bool _IsTopLevel = false;
            bool _IsEnabled = false;
            string _Name= string.Empty;
            readonly List<string> _Directories = new List<string>();
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
            _IsTopLevel = base.IsEnabled;
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
            var GroupType = GetGroupType();
            if (Changed)
            {
                base.GroupKey.SetValue(RegConfigLocations.IsTopLevelRegValueName, _IsTopLevel);
                base.GroupKey.SetValue(RegConfigLocations.DisabledRegValueName, _IsEnabled != true);
                
            }
        }

        public void ExportAsJson(Stream Output)
        {

        }

        public WritableRegGroup CreateFromJson(Stream Input)
        {
            throw new NotImplementedException("Json import not implemented yet.");
        }
        public WritableRegGroup(string FromName, IResolveIconFromPath Handler, IFolderProviderResolver FolderResolver) : base(FromName, Handler, FolderResolver, true)
        {
            
        }


    }
}
