#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov
// Source https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------
// (c) https://github.com/mikependon/Tutorials/tree/master/WPF/TreeViewFileExplorer
// (c) https://medium.com/@mikependon/designing-a-wpf-treeview-file-explorer-565a3f13f6f2
//------------------------------------------------------------------------------
#endregion License

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;

using TreeViewFileExplorer.Enums;

namespace TreeViewFileExplorer.ShellClasses
{
    public class FileSystemObjectInfo : BaseFileSystemObjectInfo
    {
        public FileSystemObjectInfo(DriveInfo drive)
            : this(drive.RootDirectory) { }

        public FileSystemObjectInfo(FileSystemInfo info)
        {
            if (this is DummyFileSystemObjectInfo)
            {
                return;
            }

            Children = new ObservableCollection<FileSystemObjectInfo>();
            FileSystemInfo = info;

            if (info is DirectoryInfo)
            {
                ImageSource = FolderManager.GetImageSource(info.FullName, ItemState.Close);
                AddDummy();
            }
            else // if (info is FileInfo)
            {
                ImageSource = FileManager.GetImageSource(info.FullName);
            }

            PropertyChanged += new PropertyChangedEventHandler(FileSystemObjectInfo_PropertyChanged);
        }

        #region Events

        public event EventHandler BeforeExpand;
        public event EventHandler AfterExpand;
        public event EventHandler BeforeExplore;
        public event EventHandler AfterExplore;

        private void RaiseBeforeExpand()
        {
            BeforeExpand?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAfterExpand()
        {
            AfterExpand?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseBeforeExplore()
        {
            BeforeExplore?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAfterExplore()
        {
            AfterExplore?.Invoke(this, EventArgs.Empty);
        }

        #endregion Events

        #region EventHandlers

        void FileSystemObjectInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (FileSystemInfo is DirectoryInfo)
            {
                if (string.Equals(e.PropertyName, nameof(IsExpanded), StringComparison.CurrentCultureIgnoreCase))
                {
                    RaiseBeforeExpand();

                    if (IsExpanded)
                    {
                        ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Open);

                        if (HasDummy())
                        {
                            RaiseBeforeExplore();

                            if (!(Drive?.IsReady == false))
                            {
                                RemoveDummy();
                                ExploreDirectories();
                                ExploreFiles();
                            }

                            RaiseAfterExplore();
                        }
                    }
                    else
                    {
                        ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Close);
                    }

                    RaiseAfterExpand();
                }
            }
        }

        #endregion EventHandlers

        #region Properties

        public ObservableCollection<FileSystemObjectInfo> Children
        {
            get => base.GetValue<ObservableCollection<FileSystemObjectInfo>>(nameof(Children));
            private set => base.SetValue(nameof(Children), value);
        }

        public ImageSource ImageSource
        {
            get => base.GetValue<ImageSource>(nameof(ImageSource));
            private set => base.SetValue(nameof(ImageSource), value);
        }

        public bool IsExpanded // base is required here for the proper action!
        {
            get => base.GetValue<bool>(nameof(IsExpanded));
            set => base.SetValue(nameof(IsExpanded), value);
        }

        public FileSystemInfo FileSystemInfo
        {
            get => base.GetValue<FileSystemInfo>(nameof(FileSystemInfo));
            private set => base.SetValue(nameof(FileSystemInfo), value);
        }

        private DriveInfo Drive
        {
            get => base.GetValue<DriveInfo>(nameof(Drive));
            set => base.SetValue(nameof(Drive), value);
        }

        #endregion Properties

        #region Methods

        private void AddDummy()
        {
            Children.Add(new DummyFileSystemObjectInfo());
        }

        private bool HasDummy()
        {
            return GetDummy() is object;
        }

        private DummyFileSystemObjectInfo GetDummy()
        {
            return Children.OfType<DummyFileSystemObjectInfo>().FirstOrDefault();
        }

        private void RemoveDummy()
        {
            _ = Children.Remove(GetDummy());
        }

        private void ExploreDirectories()
        {
            try
            {
                var info = (DirectoryInfo)FileSystemInfo;
                var directories = info.GetDirectories();

                foreach (var directory in directories.OrderBy(d => d.Name))
                {
                    if (!directory.Attributes.HasFlag(FileAttributes.System) &&
                        !directory.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        var fileSystemObject = new FileSystemObjectInfo(directory);

                        fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                        fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;

                        Children.Add(fileSystemObject);
                    }
                }
            }
            catch { } // Network disconnected
        }

        private void FileSystemObject_AfterExplore(object sender, EventArgs e)
        {
            RaiseAfterExplore();
        }

        private void FileSystemObject_BeforeExplore(object sender, EventArgs e)
        {
            RaiseBeforeExplore();
        }

        private void ExploreFiles()
        {
            try
            {
                var info = (DirectoryInfo)FileSystemInfo;
                var files = info.GetFiles();

                foreach (var file in files.OrderBy(d => d.Name))
                {
                    if (!file.Attributes.HasFlag(FileAttributes.System) &&
                        !file.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        Children.Add(new FileSystemObjectInfo(file));
                    }
                }
            }
            catch { } // Network disconnected
        }

        #endregion Methods
    }
}
