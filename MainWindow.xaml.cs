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
#endregion License

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TreeViewFileExplorer;
using TreeViewFileExplorer.ShellClasses;

using WhoCan.Models;

namespace WhoCan
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _preSelectPath = ""; //@"C:\Program Files\7-Zip\Lang"; // @"G:\"; //TODO

        public MainWindow()
        {
            InitializeComponent();
            InitializeFileSystemObjects();
            DataContext = new MainViewModel();

            Title = $"{App.Title} v{App.Version.ToString(3)}";
        }

        public void Refresh()
        {
            Cursor = Cursors.Wait;

            var context = (MainViewModel)DataContext;

            _preSelectPath = context.SelectedFileSystemInfo.FullName;
            FoldersControl.Items.Clear();
            InitializeFileSystemObjects();

            context.SetPathSelected();

            RulesControl.UpdateLayout();
            UsersControl.UpdateLayout();
            GroupsControl.UpdateLayout();

            Cursor = Cursors.Arrow;
        }

        private void InitializeFileSystemObjects()
        {
            DriveInfo
                .GetDrives()
                .ToList()
                .ForEach(drive =>
                {
                    var fileSystemObject = new FileSystemObjectInfo(drive);

                    fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                    fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;

                    _ = FoldersControl.Items.Add(fileSystemObject);
                });

            //PreSelect(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            if (!string.IsNullOrEmpty(_preSelectPath))
            {
                ViewManager.PreSelect(FoldersControl, _preSelectPath);
            }
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

        #endregion Events

        #region Selected

        private void FoldersControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (FileSystemObjectInfo)e.NewValue;

            if (item != null)
            {
                Cursor = Cursors.Wait;

                var context = (MainViewModel)DataContext;

                // binding manually
                var info = item.FileSystemInfo;
                //context.SelectedFileSystemInfo = info;
                try
                {
                    StatusLastWrite.Text = info.LastWriteTime.ToString("dd.MM.yy HH:mm");
                    StatusPath.Text = info.FullName;
                    StatusOwner.Text = context.GetOwner(info);
                    context.SetPathSelected(info);
                }
                catch { } // Network disconnected

                RulesControl.UpdateLayout();
                UsersControl.UpdateLayout();
                GroupsControl.UpdateLayout();

                Cursor = Cursors.Arrow;
            }
        }

        private void RulesControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor = Cursors.Wait;

            var context = (MainViewModel)DataContext;
            context.SetRuleSelected();
            UsersControl.UpdateLayout();
            GroupsControl.UpdateLayout();

            Cursor = Cursors.Arrow;
        }

        private void GroupsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor = Cursors.Wait;

            var context = (MainViewModel)DataContext;
            context.SetGroupSelected();
            UsersControl.UpdateLayout();

            Cursor = Cursors.Arrow;
        }

        private void UsersControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor = Cursors.Wait;

            var context = (MainViewModel)DataContext;
            context.SetUserSelected();
            GroupsControl.UpdateLayout();

            Cursor = Cursors.Arrow;
        }

        #endregion Selected

        #region Commands

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var path = FoldersControl.SelectedValuePath;
            var result = new StringBuilder();
            char tab = '\t';

            if (e.Source.Equals(FoldersControl) || e.Source.Equals(StatusPath))
            {
                if (path.Contains(" "))
                {
                    path = $"\"{path}\"";
                }

                Clipboard.SetText(path);
                e.Handled = true;
            }
            else if (e.Source.Equals(RulesControl))
            {
                _ = result
                    .Append("Ресурс").Append(tab)
                    .Append("Тип").Append(tab)
                    .Append("Аккаунт").Append(tab)
                    .Append("Права").Append(tab)
                    .AppendLine("Подробнее");

                foreach (RuleInfo item in RulesControl.Items)
                {
                    _ = result
                        .Append(path).Append(tab)
                        .Append(item.IsGroup ? "Группа" : "Пользователь").Append(tab)
                        .Append(item.PrincipalName).Append(tab)
                        .Append(item.Flags).Append(tab)
                        .AppendLine(item.Comment);
                }

                Clipboard.SetText(result.ToString());
                e.Handled = true;
            }
            else if (e.Source.Equals(UsersControl))
            {
                _ = result
                    .Append("Ресурс").Append(tab)
                    .Append("Логин").Append(tab)
                    .Append("Права").Append(tab)
                    .Append("Имя").Append(tab)
                    .Append("Фамилия").Append(tab)
                    .Append("Имя Фамилия").Append(tab)
                    .AppendLine("Подробнее");

                foreach (UserInfo item in UsersControl.Items)
                {
                    //if (item.Enabled)
                    {
                        _ = result
                            .Append(path).Append(tab)
                            .Append(item.UserName).Append(tab)
                            .Append(item.IsDanger ? "RW" : "R").Append(tab)
                            .Append(item.Name).Append(tab)
                            .Append(item.Family).Append(tab)
                            .Append(item.DisplayName).Append(tab)
                            .AppendLine(item.Comment);
                    }
                }

                Clipboard.SetText(result.ToString());
                e.Handled = true;
            }
            else if (e.Source.Equals(GroupsControl))
            {
                UserInfo userInfo = (UserInfo)UsersControl.SelectedItem;

                _ = result
                    .Append("Пользователь").Append(tab)
                    .Append("Имя Фамилия").Append(tab)
                    .Append("Группа").Append(tab)
                    .AppendLine("Подробнее");

                foreach (GroupInfo item in GroupsControl.Items)
                {
                    _ = result
                        .Append(userInfo.UserName).Append(tab)
                        .Append(userInfo.DisplayName).Append(tab)
                        .Append(item.GroupName).Append(tab)
                        .AppendLine(item.Description);
                }

                Clipboard.SetText(result.ToString());
                e.Handled = true;
            }
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(StatusPath))
            {
                e.CanExecute = FoldersControl.SelectedItem != null;
            }
            else if (e.Source.Equals(RulesControl))
            {
                e.CanExecute = RulesControl.HasItems;
            }
            else if (e.Source.Equals(UsersControl))
            {
                e.CanExecute = UsersControl.HasItems;
            }
            else if (e.Source.Equals(GroupsControl))
            {
                e.CanExecute = GroupsControl.HasItems;
            }
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(StatusPath))
            {
                var path = Clipboard.GetText();
                ViewManager.PreSelect(FoldersControl, path);
                e.Handled = true;
            }
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(StatusPath))
            {
                var path = Clipboard.GetText();

                if (path.StartsWith("\""))
                {
                    path = path.Replace("\"", string.Empty).Trim();
                }

                e.CanExecute = Directory.Exists(path) || File.Exists(path);
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(StatusPath))
            {
                var item = (FileSystemObjectInfo)FoldersControl.SelectedItem;
                var path = item.FileSystemInfo.FullName;
                _ = Process.Start(path);
                e.Handled = true;
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(StatusPath))
            {
                e.CanExecute = FoldersControl.SelectedItem != null;
            }
        }

        #endregion Commands

        #region Menu

        private void MenuRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        #endregion Menu
    }
}
