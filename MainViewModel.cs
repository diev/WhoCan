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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

using WhoCan.Models;

namespace WhoCan
{
    public class MainViewModel : BaseObject
    {
        private const string _commonNT = @"BANK\";
        private const string _commonOU = ",OU=БАНК";
        private const string _commonDC = ",DC=bank,DC=cibank,DC=ru";

        private readonly List<string> _skipAdmins;
        private readonly List<string> _skipGroups;

        private const string _rightDeny = "x";
        private const string _rightFull = "F";
        private const string _rightRead = "R";
        private const string _rightWrite = "W";

        private readonly PrincipalContext _domain = null;
        private bool _inAction = false;

        public ObservableCollection<RuleInfo> RuleInfos { get; protected set; } = new ObservableCollection<RuleInfo>();
        public ObservableCollection<UserInfo> UserInfos { get; protected set; } = new ObservableCollection<UserInfo>();
        public ObservableCollection<GroupInfo> GroupInfos { get; protected set; } = new ObservableCollection<GroupInfo>();

        public FileSystemInfo SelectedFileSystemInfo { get; protected set; }

        public MainViewModel()
        {
            try
            {
                _domain = new PrincipalContext(ContextType.Domain);

                _skipAdmins = new List<string> {
                    _commonNT + "admin",
                    @"BUILTIN\Администраторы", @"BUILTIN\Administrators",
                    @"NT AUTHORITY\СИСТЕМА", @"NT AUTHORITY\SYSTEM"
                };

                _skipGroups = new List<string> {
                    "Все",
                    "Высокий обязательный уровень",
                    "Данная организация",
                    "Подтвержденное службой удостоверение",
                    "Пользователи",
                    "Пользователи домена",
                    "Пользователи удаленного рабочего стола",
                    "Прошедшие проверку",
                    "Средний обязательный уровень"
                };
            }
            catch
            {
                _domain = new PrincipalContext(ContextType.Machine);
                _skipAdmins = new List<string>();
                _skipGroups = new List<string>();
            }
        }

        #region Public methods

        public void SetPathSelected()
        {
            SetPathSelected(SelectedFileSystemInfo);
        }

        public void SetPathSelected(FileSystemInfo info)
        {
            if (_inAction)
            {
                return;
            }

            _inAction = true;

            RuleInfos.Clear();
            UserInfos.Clear();
            GroupInfos.Clear();

            SelectedFileSystemInfo = info;

            try
            {
                AuthorizationRuleCollection rules;

                if (info is DirectoryInfo)
                {
                    DirectorySecurity security = Directory.GetAccessControl(info.FullName);
                    rules = security.GetAccessRules(true, true, typeof(NTAccount));
                }
                else // if (info is FileInfo)
                {
                    FileSecurity security = File.GetAccessControl(info.FullName);
                    rules = security.GetAccessRules(true, true, typeof(NTAccount));
                }

                foreach (FileSystemAccessRule rule in rules)
                {
                    var name = rule.IdentityReference.Value;

                    if (_skipAdmins.Contains(name))
                    {
                        continue;
                    }

                    bool deny = rule.AccessControlType.HasFlag(AccessControlType.Deny);
                    bool danger = false;
                    var flags = new StringBuilder();

                    if (deny)
                    {
                        _ = flags.Append(_rightDeny);

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Write | FileSystemRights.Delete))
                        {
                            _ = flags.Append(_rightWrite);
                        }
                    }
                    else
                    {
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.FullControl))
                        {
                            danger = true;
                            _ = flags.Append(_rightFull);
                        }

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute))
                        {
                            _ = flags.Append(_rightRead);
                        }

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Modify | FileSystemRights.Delete))
                        {
                            danger = true;
                            _ = flags.Append(_rightWrite);
                        }
                    }

                    var principal = Principal.FindByIdentity(_domain, name); //TODO long!
                    bool isGroup = principal is GroupPrincipal; // principal.IsSecurityGroup?

                    var ruleInfo = new RuleInfo
                    {
                        Comment = GetRightsEnum(rule),
                        Deny = deny,
                        Domain = name.StartsWith(_commonNT),
                        Flags = flags.ToString(),
                        IsDanger = danger,
                        IsGroup = isGroup,
                        IsInherited = rule.IsInherited,
                        IsSelected = false,
                        Principal = principal,
                        PrincipalName = name.Replace(_commonNT, string.Empty)
                    };

                    RuleInfos.Add(ruleInfo);
                    AddRuleUsers(ruleInfo);

                    if (isGroup)
                    {
                        AddNestedGroups(ruleInfo);
                    }
                }
            }
            catch { }

            _inAction = false;
        }

        private string GetRightsEnum(FileSystemAccessRule rule)
        {
            var rights = new StringBuilder();

            if (rule.AccessControlType.HasFlag(AccessControlType.Deny))
            {
                _ = rights.Append("Deny ");
            }

            //Specifies the right to append data to the end of a file.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.AppendData))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.AppendData));
            //}

            //Specifies the right to change the security and audit rules associated with a file or folder.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.ChangePermissions))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.ChangePermissions));
            //}

            //Specifies the right to create a folder This right requires the Synchronize value.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.CreateDirectories));
            }

            //Specifies the right to create a file. This right requires the Synchronize value.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.CreateFiles));
            }

            //Specifies the right to delete a folder or file.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.Delete))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.Delete));
            }

            //Specifies the right to delete a folder and any files contained within that folder.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.DeleteSubdirectoriesAndFiles))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.DeleteSubdirectoriesAndFiles));
            }

            //Specifies the right to run an application file.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.ExecuteFile))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.ExecuteFile));
            //}

            //Specifies the right to exert full control over a folder or file,
            //and to modify access control and audit rules. This value represents the right
            //to do anything with a file and is the combination of all rights in this enumeration.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.FullControl))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.FullControl));
            }

            //Specifies the right to read the contents of a directory.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.ListDirectory))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ListDirectory));
            }

            //Specifies the right to read, write, list folder contents,
            //delete folders and files, and run application files.
            //This right includes the ReadAndExecute right, the Write right, and the Delete right.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.Modify))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.Modify));
            }

            //Specifies the right to open and copy folders or files as read-only.
            //This right includes the ReadData right, ReadExtendedAttributes right,
            //ReadAttributes right, and ReadPermissions right.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.Read))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.Read));
            }

            //Specifies the right to open and copy folders or files as read-only,
            //and to run application files. This right includes the Read right and the ExecuteFile right.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.ReadAndExecute));
            //}

            //Specifies the right to open and copy file system attributes from a folder or file.
            //For example, this value specifies the right to view the file creation or modified date.
            //This does not include the right to read data, extended file system attributes, or access and audit rules.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.ReadAttributes));
            //}

            //Specifies the right to open and copy a file or folder.
            //This does not include the right to read file system attributes,
            //extended file system attributes, or access and audit rules.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadData))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.ReadData));
            //}

            //Specifies the right to open and copy extended file system attributes from a folder or file.
            //For example, this value specifies the right to view author and content information.
            //This does not include the right to read data, file system attributes, or access and audit rules.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadExtendedAttributes))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.ReadExtendedAttributes));
            //}

            //Specifies the right to open and copy access and audit rules from a folder or file.
            //This does not include the right to read data, file system attributes,
            //and extended file system attributes.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadPermissions))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.ReadPermissions));
            //}

            //Specifies whether the application can wait for a file handle to synchronize
            //with the completion of an I/O operation.
            //This value is automatically set when allowing access and automatically excluded when denying access.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.Synchronize))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.Synchronize));
            //}

            //Specifies the right to change the owner of a folder or file.
            //Note that owners of a resource have full access to that resource.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.TakeOwnership))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.TakeOwnership));
            //}

            //Specifies the right to list the contents of a folder and to run applications contained within that folder.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.Traverse))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.Traverse));
            }

            //Specifies the right to create folders and files, and to add or remove data from files.
            //This right includes the WriteData right, AppendData right, WriteExtendedAttributes right,
            //and WriteAttributes right.
            if (rule.FileSystemRights.HasFlag(FileSystemRights.Write))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.Write));
            }

            //Specifies the right to open and write file system attributes to a folder or file.
            //This does not include the ability to write data, extended attributes, or access and audit rules.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.WriteAttributes));
            //}

            //Specifies the right to open and write to a file or folder.
            //This does not include the right to open and write file system attributes,
            //extended file system attributes, or access and audit rules.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.WriteData))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.WriteData));
            //}

            //Specifies the right to open and write extended file system attributes to a folder or file.
            //This does not include the ability to write data, attributes, or access and audit rules.
            //if (rule.FileSystemRights.HasFlag(FileSystemRights.WriteExtendedAttributes))
            //{
            //    _ = rights.AppendLine(nameof(FileSystemRights.WriteExtendedAttributes));
            //}

            string comment = rights.ToString().Replace(Environment.NewLine, ", ").Trim();

            return comment.EndsWith(",")
                ? comment.Substring(0, comment.Length - 1)
                : comment;
        }

        public void SetRuleSelected()
        {
            if (_inAction)
            {
                return;
            }

            _inAction = true;

            UserInfos.Clear();
            GroupInfos.Clear();

            foreach (var item in RuleInfos)
            {
                if (item.IsSelected)
                {
                    AddRuleUsers(item);

                    if (item.IsGroup)
                    {
                        AddNestedGroups(item);
                    }
                }
            }

            _inAction = false;
        }

        public void SetUserSelected()
        {
            if (_inAction)
            {
                return;
            }

            _inAction = true;

            GroupInfos.Clear();

            foreach (var item in UserInfos)
            {
                if (item.IsSelected)
                {
                    AddUserGroups(item);
                }
            }

            _inAction = false;
        }

        public void SetGroupSelected()
        {
            if (_inAction)
            {
                return;
            }

            _inAction = true;

            UserInfos.Clear();

            foreach (var item in GroupInfos)
            {
                if (item.IsSelected)
                {
                    AddGroupUsers(item);
                }
            }

            _inAction = false;
        }

        #endregion Public methods

        #region Users

        private void AddRuleUsers(RuleInfo item)
        {
            if (item.Principal != null)
            {
                if (item.IsGroup)
                {
                    var members = ((GroupPrincipal)item.Principal).GetMembers(true);

                    foreach (Principal member in members)
                    {
                        if (member is UserPrincipal user)
                        {
                            AddRuleUser(item, user);
                        }
                    }
                }
                else
                {
                    AddRuleUser(item, (UserPrincipal)item.Principal);
                }
            }
        }

        private void AddGroupUsers(GroupInfo item)
        {
            var members = item.GroupPrincipal.GetMembers(true);

            foreach (Principal member in members)
            {
                if (member is UserPrincipal user)
                {
                    AddGroupUser(item, user);
                }
            }
        }

        private void AddRuleUser(RuleInfo item, UserPrincipal user)
        {
            string comment = user.DistinguishedName == null
                ? string.Empty
                : user.DistinguishedName // "CN=Name Surname,OU=Dept,OU=XXXX,DC=XXX,DC=XXXX,DC=XX" -> "Name Surname, Dept"
                    .Replace("CN=", string.Empty)
                    .Replace(_commonOU, string.Empty)
                    .Replace(_commonDC, string.Empty)
                    .Replace("OU=", " ");

            var userInfo = new UserInfo
            {
                Comment = comment,
                DisplayName = user.DisplayName,
                Enabled = (bool)user.Enabled,
                Family = user.Surname,
                IsDanger = (bool)user.Enabled && item.IsDanger,
                IsSelected = false,
                Name = user.GivenName,
                UserName = user.SamAccountName,
                UserPrincipal = user
            };

            int i = UserInfos.IndexOf(userInfo);

            if (i < 0 && !item.Deny)
            {
                UserInfos.Add(userInfo); 
            }
            else if (UserInfos[i].Enabled && !UserInfos[i].IsDanger && userInfo.IsDanger)
            {
                UserInfos[i].IsDanger = true;
            }
        }

        private void AddGroupUser(GroupInfo item, UserPrincipal user)
        {
            string comment = user.DistinguishedName == null
                ? string.Empty
                : user.DistinguishedName // "CN=Name Surname,OU=Dept,OU=XXXX,DC=XXX,DC=XXXX,DC=XX" -> "Name Surname, Dept"
                    .Replace("CN=", string.Empty)
                    .Replace(_commonOU, string.Empty)
                    .Replace(_commonDC, string.Empty)
                    .Replace("OU=", " ");

            var userInfo = new UserInfo
            {
                Comment = comment,
                DisplayName = user.DisplayName,
                Enabled = (bool)user.Enabled,
                Family = user.Surname,
                IsDanger = false, //(bool)user.Enabled && item.IsDanger,
                IsSelected = false,
                Name = user.GivenName,
                UserName = user.SamAccountName,
                UserPrincipal = user
            };

            if (!UserInfos.Contains(userInfo))
            {
                UserInfos.Add(userInfo);
            }
        }

        //private void AddLocalUser(string principal)
        //{
        //    string group, user;

        //    if (principal.Contains("\\"))
        //    {
        //        string[] account = principal.Split('\\');
        //        group = account[0];
        //        user = account[1];
        //    }
        //    else
        //    {
        //        group = string.Empty;
        //        user = principal;
        //    }

        //    var userInfo = new UserInfo(
        //        user,
        //        false,
        //        principal,
        //        user,
        //        group,
        //        Environment.MachineName + ", " + principal,
        //        false
        //    );

        //    if (!UserInfos.Contains(userInfo))
        //    {
        //        UserInfos.Add(userInfo);
        //    }
        //}

        #endregion Users

        #region Groups

        private void AddNestedGroups(RuleInfo item)
        {
            if (item.Principal != null)
            {
                var members = ((GroupPrincipal)item.Principal).GetMembers(true); //TODO recursive doesn't work - help required!

                foreach (Principal member in members)
                {
                    if (member is GroupPrincipal group)
                    {
                        if (_skipGroups.Contains(group.Name))
                        {
                            continue;
                        }

                        var groupInfo = new GroupInfo
                        {
                            Description = group.Description ?? group.DisplayName,
                            GroupName = group.Name,
                            GroupPrincipal = group,
                            //IsDanger = item.IsDanger,
                            IsSelected = false
                        };

                        if (!GroupInfos.Contains(groupInfo))
                        {
                            GroupInfos.Add(groupInfo);
                        }
                    }
                }
            }
        }

        private void AddUserGroups(UserInfo item)
        {
            //if (item.UserPrincipal != null)
            {
                var groups = item.UserPrincipal.GetAuthorizationGroups();

                if (groups != null)
                {
                    // iterate over all groups
                    foreach (Principal member in groups)
                    {
                        // make sure to add only group principals
                        if (member is GroupPrincipal group)
                        {
                            if (_skipGroups.Contains(group.Name))
                            {
                                continue;
                            }

                            var groupInfo = new GroupInfo
                            {
                                Description = group.Description ?? group.DisplayName,
                                GroupName = group.Name,
                                GroupPrincipal = group,
                                //IsDanger = item.IsDanger,
                                IsSelected = false
                            };

                            GroupInfos.Add(groupInfo);
                        }
                    }
                }
            }
        }

        #endregion Groups

        #region Status

        public string GetOwner(FileSystemInfo info)
        {
            string owner = string.Empty;

            try
            {
                if (info is DirectoryInfo)
                {
                    DirectorySecurity security = Directory.GetAccessControl(info.FullName);
                    owner = security.GetOwner(typeof(NTAccount)).ToString();
                }
                else // if (info is FileInfo)
                {
                    FileSecurity security = File.GetAccessControl(info.FullName);
                    owner = security.GetOwner(typeof(NTAccount)).ToString();
                }

                var principal = Principal.FindByIdentity(_domain, owner); // TODO long!

                if (principal != null)
                {
                    string name = principal.DisplayName;

                    if (!string.IsNullOrEmpty(name))
                    {
                        owner += $" ({name})";
                    }
                }
            }
            catch { }

            return owner;
        }

        #endregion Status
    }
}
