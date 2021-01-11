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
#endregion

using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TreeViewFileExplorer;
using TreeViewFileExplorer.ShellClasses;

namespace WhoCan
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeFileSystemObjects();

            DataContext = new MainViewModel();
        }

        private void InitializeFileSystemObjects()
        {
            var drives = DriveInfo.GetDrives();
            DriveInfo
                .GetDrives()
                .ToList()
                .ForEach(drive =>
                {
                    var fileSystemObject = new FileSystemObjectInfo(drive);
                    fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                    fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                    FoldersControl.Items.Add(fileSystemObject);
                });
            //PreSelect(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            ViewManager.PreSelect(FoldersControl, @"G:\");
        }

        #region Events

        private void FileSystemObject_AfterExplore(object sender, EventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void FileSystemObject_BeforeExplore(object sender, EventArgs e)
        {
            Cursor = Cursors.Wait;
        }

        #endregion

        private void FoldersControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Cursor = Cursors.Wait;
            var item = (FileSystemObjectInfo)e.NewValue;
            var info = item.FileSystemInfo;
            string path = info.FullName;
            
            Title = $"WhoCan | {path}";

            string owner;
            try
            {
                if (info is DirectoryInfo)
                {
                    DirectorySecurity security = Directory.GetAccessControl(path);
                    owner = security.GetOwner(typeof(NTAccount)).ToString();
                }
                else // if (info is FileInfo)
                {
                    FileSecurity security = File.GetAccessControl(path);
                    owner = security.GetOwner(typeof(NTAccount)).ToString();
                }
            }
            catch
            {
                owner = "?";
            }
            Status.Text = $"Владелец: {owner}";

            RulesControl.ItemsSource = null;
            RulesControl.Items.Clear();

            ((MainViewModel)DataContext).SelectedFileSystemObjectInfo = item;
            RulesControl.ItemsSource = ((MainViewModel)DataContext).GetRuleInfos();
            RulesControl.UpdateLayout();

            UsersControl.ItemsSource = null;
            UsersControl.Items.Clear();

            //if (!lightest) //TODO: Add this Option
            {
                UsersControl.ItemsSource = ((MainViewModel)DataContext).GetAllUserInfos();
            }
            UsersControl.UpdateLayout();

            GroupsControl.ItemsSource = null;
            GroupsControl.Items.Clear();
            GroupsControl.UpdateLayout();

            Cursor = Cursors.Arrow;
        }

        private void RulesControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor = Cursors.Wait;
            UsersControl.ItemsSource = null;
            UsersControl.Items.Clear();

            UsersControl.ItemsSource = ((MainViewModel)DataContext).GetUserInfos();
            UsersControl.UpdateLayout();
            Cursor = Cursors.Arrow;
        }

        private void UsersControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor = Cursors.Wait;
            GroupsControl.ItemsSource = null;
            GroupsControl.Items.Clear();
            try
            {
                Models.UserInfo item = (Models.UserInfo)UsersControl.SelectedItem;
                if (item != null)
                {
                    GroupsControl.ItemsSource = ((MainViewModel)DataContext).GetGroupInfos(item.UserName);
                }
            }
            catch { }
            GroupsControl.UpdateLayout();
            Cursor = Cursors.Arrow;
        }
    }
}
