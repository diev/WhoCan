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
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
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
        private string PreSelectPath = ""; //@"C:\Program Files\7-Zip\Lang"; // @"G:\"; //TODO

        public MainWindow()
        {
            InitializeComponent();
            InitializeFileSystemObjects();

            Title = $"{App.Title} v{App.Version.ToString(3)}";
        }

        public void Refresh()
        {
            Cursor = Cursors.Wait;

            PreSelectPath = FoldersControl.SelectedValuePath;
            FoldersControl.Items.Clear();
            InitializeFileSystemObjects();

            //var context = (MainViewModel)DataContext;

            RulesControl.ItemsSource = null;
            //RulesControl.Items.Clear();
            //RulesControl.UpdateLayout();

            UsersControl.ItemsSource = null;
            //UsersControl.Items.Clear();
            //UsersControl.UpdateLayout();

            GroupsControl.ItemsSource = null;
            //GroupsControl.Items.Clear();
            //GroupsControl.UpdateLayout();

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
                    FoldersControl.Items.Add(fileSystemObject);
                });

            //PreSelect(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            if (!string.IsNullOrEmpty(PreSelectPath))
            {
                ViewManager.PreSelect(FoldersControl, PreSelectPath);
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

        #endregion

        private void FoldersControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (FileSystemObjectInfo)e.NewValue;
            if (item == null) return;

            Cursor = Cursors.Wait;

            var info = item.FileSystemInfo;
            string path = info.FullName;
            FoldersControl.SelectedValuePath = path;
            string owner = string.Empty;

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

                PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);
                var principal = Principal.FindByIdentity(principalContext, owner);
                string name = principal.DisplayName;

                if (name.Length > 0)
                {
                    owner += $" ({name})";
                }
            }
            catch { }

            Path.Text = path;
            PathOwner.Text = owner;
            LastWrite.Text = info.LastWriteTime.ToString();

            RulesControl.ItemsSource = null;
            RulesControl.Items.Clear();

            var context = (MainViewModel)DataContext;
            context.SelectedFileSystemObjectInfo = item;

            RulesControl.ItemsSource = context.GetRuleInfos();
            RulesControl.UpdateLayout();

            UsersControl.ItemsSource = null;
            UsersControl.Items.Clear();

            //if (!lightest) //TODO: Add this Option
            //{
                UsersControl.ItemsSource = context.GetAllUserInfos();
            //}
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

            var context = (MainViewModel)DataContext;
            UsersControl.ItemsSource = context.GetUserInfos();
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
                UserInfo item = (Models.UserInfo)UsersControl.SelectedItem;

                if (item != null)
                {
                    var context = (MainViewModel)DataContext;
                    GroupsControl.ItemsSource = context.GetGroupInfos(item.UserName);
                }
            }
            catch { }

            GroupsControl.UpdateLayout();
            Cursor = Cursors.Arrow;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var path = FoldersControl.SelectedValuePath;
            var result = new StringBuilder();
            char tab = '\t';

            if (e.Source.Equals(FoldersControl) || e.Source.Equals(Path))
            {
                if (path.Contains(" "))
                {
                    path = $"\"{path}\"";
                }

                Clipboard.SetText(path);
                e.Handled = true;

                return;
            }

            if (e.Source.Equals(RulesControl))
            {
                result.Append("Ресурс").Append(tab);
                result.Append("Тип").Append(tab);
                result.Append("Аккаунт").Append(tab);
                result.Append("Права").Append(tab);
                result.AppendLine("Подробнее");

                foreach (RuleInfo item in RulesControl.Items)
                {
                    result.Append(path).Append(tab);
                    result.Append(item.IsGroup ? "Группа" : "Пользователь").Append(tab);
                    result.Append(item.PrincipalName).Append(tab);
                    result.Append(item.Flags).Append(tab);
                    result.AppendLine(item.Comment);
                }

                Clipboard.SetText(result.ToString());
                e.Handled = true;

                return;
            }

            if (e.Source.Equals(UsersControl))
            {
                result.Append("Ресурс").Append(tab);
                result.Append("Логин").Append(tab);
                result.Append("Права").Append(tab);
                result.Append("Имя").Append(tab);
                result.Append("Фамилия").Append(tab);
                result.Append("Имя Фамилия").Append(tab);
                result.AppendLine("Подробнее");

                foreach (UserInfo item in UsersControl.Items)
                {
                    //if (item.Enabled)
                    {
                        result.Append(path).Append(tab);
                        result.Append(item.UserName).Append(tab);
                        result.Append(item.IsDanger ? "RW" : "R").Append(tab);
                        result.Append(item.Name).Append(tab);
                        result.Append(item.Family).Append(tab);
                        result.Append(item.DisplayName).Append(tab);
                        result.AppendLine(item.Comment);
                    }
                }

                Clipboard.SetText(result.ToString());
                e.Handled = true;

                return;
            }

            if (e.Source.Equals(GroupsControl))
            {
                UserInfo userInfo = (UserInfo)UsersControl.SelectedItem;

                result.Append("Пользователь").Append(tab);
                result.Append("Имя Фамилия").Append(tab);
                result.Append("Группа").Append(tab);
                result.AppendLine("Подробнее");

                foreach (GroupInfo item in GroupsControl.Items)
                {
                    result.Append(userInfo.UserName).Append(tab);
                    result.Append(userInfo.DisplayName).Append(tab);
                    result.Append(item.GroupName).Append(tab);
                    result.AppendLine(item.Description);
                }

                Clipboard.SetText(result.ToString());
                e.Handled = true;

                return;
            }
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(Path))
            {
                e.CanExecute = FoldersControl.SelectedItem != null;
            }

            if (e.Source.Equals(RulesControl))
            {
                e.CanExecute = RulesControl.HasItems;
            }

            if (e.Source.Equals(UsersControl))
            {
                e.CanExecute = UsersControl.HasItems;
            }

            if (e.Source.Equals(GroupsControl))
            {
                e.CanExecute = GroupsControl.HasItems;
            }
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(Path))
            {
                var path = Clipboard.GetText();
                ViewManager.PreSelect(FoldersControl, path);
                e.Handled = true;

                return;
            }
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.Equals(FoldersControl) || e.Source.Equals(Path))
            {
                var path = Clipboard.GetText();

                if (path.StartsWith("\""))
                {
                    path = path.Replace("\"", string.Empty).Trim();
                }

                e.CanExecute = Directory.Exists(path) || File.Exists(path);
            }
        }

        private void MenuRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
