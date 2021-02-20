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
#endregion

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;

using TreeViewFileExplorer.Enums;

using WhoCan.Models; // BaseObject

namespace TreeViewFileExplorer.ShellClasses
{
    public class FileSystemObjectInfo : BaseObject
    {
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

        public FileSystemObjectInfo(DriveInfo drive) : this(drive.RootDirectory) { }

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

        #endregion

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

                            RemoveDummy();
                            ExploreDirectories();
                            ExploreFiles();

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

        #endregion

        #region Properties

        public ObservableCollection<FileSystemObjectInfo> Children
        {
            get => GetValue<ObservableCollection<FileSystemObjectInfo>>(nameof(Children));
            private set => SetValue(nameof(Children), value);
        }

        public ImageSource ImageSource
        {
            get => GetValue<ImageSource>(nameof(ImageSource));
            private set => SetValue(nameof(ImageSource), value);
        }

        public bool IsExpanded
        {
            get => GetValue<bool>(nameof(IsExpanded));
            set => SetValue(nameof(IsExpanded), value);
        }

        public FileSystemInfo FileSystemInfo
        {
            get => GetValue<FileSystemInfo>(nameof(FileSystemInfo));
            private set => SetValue(nameof(FileSystemInfo), value);
        }

        private DriveInfo Drive
        {
            get => GetValue<DriveInfo>(nameof(Drive));
            set => SetValue(nameof(Drive), value);
        }

        #endregion

        #region Methods

        private void AddDummy()
        {
            Children.Add(new DummyFileSystemObjectInfo());
        }

        private bool HasDummy()
        {
            return GetDummy() != null;
        }

        private DummyFileSystemObjectInfo GetDummy()
        {
            return Children.OfType<DummyFileSystemObjectInfo>().FirstOrDefault();
        }

        private void RemoveDummy()
        {
            Children.Remove(GetDummy());
        }

        private void ExploreDirectories()
        {
            if (Drive?.IsReady != false)
            {
                if (FileSystemInfo is DirectoryInfo info)
                {
                    try
                    {
                        var directories = info.GetDirectories().OrderBy(d => d.Name);

                        foreach (var dir in directories)
                        {
                            if (!dir.Attributes.HasFlag(FileAttributes.System | FileAttributes.Hidden))
                            {
                                var fileSystemObject = new FileSystemObjectInfo(dir);

                                fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                                fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;

                                Children.Add(fileSystemObject);
                            }
                        }
                    }
                    catch { } //UnauthorizedAccessException!
                }
            }
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
            if (Drive?.IsReady != false)
            {
                if (FileSystemInfo is DirectoryInfo info)
                {
                    try
                    {
                        var files = info.EnumerateFiles().OrderBy(d => d.Name);

                        foreach (var file in files)
                        {
                            if (!file.Attributes.HasFlag(FileAttributes.System | FileAttributes.Hidden))
                            {
                                Children.Add(new FileSystemObjectInfo(file));
                            }
                        }
                    }
                    catch { } //UnauthorizedAccessException!
                }
            }
        }

        #endregion
    }
}
