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
using System.Diagnostics;
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
        private const string _rightDeny = "x";
        private const string _rightFull = "F";
        private const string _rightRead = "R";
        private const string _rightWrite = "W";
        private const string _rightTransit = "T";

        private bool _inAction = false;

        public ObservableCollection<RuleInfo> RuleInfos { get; protected set; } = new ObservableCollection<RuleInfo>();
        public ObservableCollection<UserInfo> UserInfos { get; protected set; } = new ObservableCollection<UserInfo>();
        public ObservableCollection<GroupInfo> GroupInfos { get; protected set; } = new ObservableCollection<GroupInfo>();

        public FileSystemInfo SelectedFileSystemInfo { get; protected set; }

        public MainViewModel()
        {
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
                bool container = info is DirectoryInfo;

                if (container)
                {
                    DirectorySecurity security = Directory.GetAccessControl(info.FullName);
                    rules = security.GetAccessRules(true, true, typeof(NTAccount));
                }
                else
                {
                    FileSecurity security = File.GetAccessControl(info.FullName);
                    rules = security.GetAccessRules(true, true, typeof(NTAccount));
                }

                foreach (FileSystemAccessRule rule in rules)
                {
                    var identityValue = rule.IdentityReference.Value;
                    var principal = Helpers.FindByIdentity(identityValue);
                    bool isGroup = principal is GroupPrincipal; // principal.IsSecurityGroup?

                    if (principal == null) // user "NT AUTHORITY\"
                    {
                        continue;
                    }

                    string name = isGroup ? principal.Name : principal.SamAccountName;

                    if (Helpers.IsSystemName(isGroup, name))
                    {
                        continue;
                    }

                    bool deny = rule.AccessControlType.HasFlag(AccessControlType.Deny);
                    bool danger = false;
                    bool transit = false;
                    var flags = new StringBuilder();

                    if (deny)
                    {
                        _ = flags.Append(_rightDeny);

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Write) ||
                            rule.FileSystemRights.HasFlag(FileSystemRights.Delete) ||
                            rule.FileSystemRights.HasFlag(FileSystemRights.DeleteSubdirectoriesAndFiles))
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
                            if (container && rule.InheritanceFlags.Equals(InheritanceFlags.None))
                            {
                                transit = true;
                                _ = flags.Append(_rightTransit);
                            }
                            else
                            {
                                _ = flags.Append(_rightRead);
                            }
                        }

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Modify) ||
                            rule.FileSystemRights.HasFlag(FileSystemRights.Delete))
                        {
                            danger = true;
                            _ = flags.Append(_rightWrite);
                        }
                    }

                    string domain = Environment.UserDomainName;
                   
                    var ruleInfo = new RuleInfo
                    {
                        Comment = Helpers.GetRightsEnum(rule),
                        Deny = deny,
                        Domain = identityValue.StartsWith(domain),
                        Flags = flags.ToString(),
                        IsDanger = danger,
                        IsGroup = isGroup,
                        IsInherited = rule.IsInherited,
                        IsSelected = false,
                        IsTransit = transit,
                        Principal = principal,
                        PrincipalName = name,
                        Rule = rule
                    };

                    RuleInfos.Add(ruleInfo);

                    //if (principal != null) // continue above
                    //{
                        AddRuleUsers(ruleInfo);
                    //}

                    if (isGroup)
                    {
                        AddNestedGroups(ruleInfo);
                    }
                }
            }
            catch { }

            _inAction = false;
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
                    if (item.Principal != null)
                    {
                        AddRuleUsers(item);
                    }

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
            if (item.IsGroup)
            {
                var members = ((GroupPrincipal)item.Principal).GetMembers(true);

                try
                {
                    foreach (Principal member in members)
                    {
                        if (member is UserPrincipal user)
                        {
                            AddRuleUser(item, user);
                        }
                    }
                }
                catch { } // No network
            }
            else
            {
                AddRuleUser(item, (UserPrincipal)item.Principal);
            }
        }

        private void AddGroupUsers(GroupInfo item)
        {
            var members = item.GroupPrincipal.GetMembers(true);

            try
            {
                foreach (Principal member in members)
                {
                    if (member is UserPrincipal user)
                    {
                        AddGroupUser(item, user);
                    }
                }
            }
            catch { } // No network
        }

        private void AddRuleUser(RuleInfo item, UserPrincipal user)
        {
            var userInfo = new UserInfo
            {
                Comment = Helpers.DistinguishedName(user.DistinguishedName),
                DisplayName = user.DisplayName,
                Enabled = (bool)user.Enabled,
                Family = user.Surname,
                IsDanger = (bool)user.Enabled && item.IsDanger,
                IsSelected = false,
                IsTransit = item.IsTransit,
                Name = user.GivenName,
                UserName = user.SamAccountName,
                UserPrincipal = user
            };

            int i = UserInfos.IndexOf(userInfo);

            if (i < 0)
            {
                if (!item.Deny)
                {
                    UserInfos.Add(userInfo);
                }
            }
            else if (UserInfos[i].Enabled && !UserInfos[i].IsDanger && userInfo.IsDanger)
            {
                UserInfos[i].IsDanger = true;
            }
        }

        private void AddGroupUser(GroupInfo item, UserPrincipal user)
        {
            var userInfo = new UserInfo
            {
                Comment = Helpers.DistinguishedName(user.DistinguishedName),
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
            Trace.Assert(item.Principal != null, "No RuleInfo.Pricipal");

            var members = ((GroupPrincipal)item.Principal).GetMembers(); //TODO recursive doesn't work - help required!

            try
            {
                foreach (Principal member in members)
                {
                    if (member is GroupPrincipal group)
                    {
                        if (Helpers.IsSystemName(true, group.Name))
                        {
                            continue;
                        }

                        var groupInfo = new GroupInfo
                        {
                            Description = group.Description ?? group.DisplayName ?? group.SamAccountName,
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
            catch { } // No network
        }

        private void AddUserGroups(UserInfo item)
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
                        if (Helpers.IsSystemName(true, group.Name))
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

                var principal = Helpers.FindByIdentity(owner);

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
