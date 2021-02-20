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

using System.IO;
using System.Linq;
using System.Windows.Controls;

using TreeViewFileExplorer.ShellClasses;

namespace TreeViewFileExplorer
{
    public static class ViewManager
    {
        private static bool _found;

        #region Methods

        public static void PreSelect(TreeView tree, string path)
        {
            if (path.StartsWith("\""))
            {
                path = path.Replace("\"", string.Empty).Trim();
            }

            var driveInfo = GetDriveFileSystemObjectInfo(tree, path);
            driveInfo.IsExpanded = true;
            _found = false;

            PreSelect(driveInfo, path);
        }

        public static void PreSelect(FileSystemObjectInfo info, string path)
        {
            foreach (var childInfo in info.Children)
            {
                if (path.StartsWith(childInfo.FileSystemInfo.FullName))
                {
                    if (childInfo.FileSystemInfo.FullName.Equals(path))
                    {
                        _found = true;
                        return; /* We found the item for pre-selection */
                    }

                    childInfo.IsExpanded = true;
                    PreSelect(childInfo, path);
                    if (_found) return;
                }
            }
        }

        #endregion
        #region Helpers

        private static FileSystemObjectInfo GetDriveFileSystemObjectInfo(TreeView tree, string path)
        {
            var directory = new DirectoryInfo(path);
            var drive = DriveInfo
                .GetDrives()
                .Where(d => d.RootDirectory.FullName == directory.Root.FullName)
                .FirstOrDefault();

            return GetDriveFileSystemObjectInfo(tree, drive);
        }

        private static FileSystemObjectInfo GetDriveFileSystemObjectInfo(TreeView tree, DriveInfo drive)
        {
            foreach (var fso in tree.Items.OfType<FileSystemObjectInfo>())
            {
                if (fso.FileSystemInfo.FullName == drive.RootDirectory.FullName)
                {
                    return fso;
                }
            }

            return null;
        }

        #endregion
    }
}
