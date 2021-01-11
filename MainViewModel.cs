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
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using TreeViewFileExplorer.ShellClasses;
using WhoCan.Models;

namespace WhoCan
{
    public class MainViewModel : BaseObject
    {
        public ObservableCollection<RuleInfo> RuleInfos { get; set; }
        public ObservableCollection<UserInfo> UserInfos { get; set; }

        public MainViewModel()
        {
            RuleInfos = new ObservableCollection<RuleInfo>();
            UserInfos = new ObservableCollection<UserInfo>();
        }

        public FileSystemObjectInfo SelectedFileSystemObjectInfo
        {
            get => GetValue<FileSystemObjectInfo>(nameof(SelectedFileSystemObjectInfo));
            set => SetValue(nameof(SelectedFileSystemObjectInfo), value);
        }

        public UserInfo SelectedUserInfo
        {
            get => GetValue<UserInfo>(nameof(SelectedUserInfo));
            set => SetValue(nameof(SelectedUserInfo), value);
        }

        public ObservableCollection<RuleInfo> GetRuleInfos()
        {
            RuleInfos.Clear();
            try
            {
                var info = SelectedFileSystemObjectInfo.FileSystemInfo;
                string path = info.FullName;
                AuthorizationRuleCollection rules;
                if (info is DirectoryInfo)
                {
                    DirectorySecurity security = Directory.GetAccessControl(path);
                    rules = security.GetAccessRules(true, true, typeof(NTAccount));
                }
                else // if (info is FileInfo)
                {
                    FileSecurity security = File.GetAccessControl(path);
                    rules = security.GetAccessRules(true, true, typeof(NTAccount));
                }

                foreach (FileSystemAccessRule rule in rules)
                {
                    bool deny = rule.AccessControlType.HasFlag(AccessControlType.Deny);
                    var flags = new StringBuilder();
                    string comment;
                    if (deny)
                    {
                        flags.Append("x");
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Write))
                            flags.Append("W");
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Delete))
                            flags.Append("D");

                        comment = "Deny " + rule.FileSystemRights.ToString();
                    }
                    else
                    {
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.FullControl))
                            flags.Append("F");
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute))
                            flags.Append("R");
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Modify))
                            flags.Append("W");
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Delete))
                            flags.Append("D");

                        comment = rule.FileSystemRights.ToString();
                    }

                    RuleInfos.Add(new RuleInfo(
                        rule.IdentityReference.Value,
                        rule.IdentityReference.Value.StartsWith(@"BANK\"),
                        deny,
                        flags.ToString(),
                        comment)
                    );
                }
            }
            catch { }
            return RuleInfos;
        }

        public ObservableCollection<UserInfo> GetUserInfos()
        {
            UserInfos.Clear();
            try
            {
                PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);
                foreach (var ruleInfo in RuleInfos)
                {
                    if (ruleInfo.IsSelected)
                    {
                        var principal = Principal.FindByIdentity(principalContext, ruleInfo.PrincipalName);
                        if (principal is GroupPrincipal group)
                        {
                            var members = group.GetMembers(true);
                            foreach (Principal member in members)
                            {
                                if (member is UserPrincipal user1)
                                {
                                    AddDomainUser(user1);
                                }
                            }
                        }
                        if (principal is UserPrincipal user)
                        {
                            AddDomainUser(user);
                        }
                    }
                }
            }
            catch
            {
                foreach (var ruleInfo in RuleInfos)
                {
                    if (ruleInfo.IsSelected)
                    {
                        AddLocalUser(ruleInfo.PrincipalName);
                    }
                }
            }
            return UserInfos;
        }

        private void AddDomainUser(UserPrincipal u)
        {
            var userInfo = new UserInfo(
                u.SamAccountName, //.UserPrincipalName,
                (bool)u.Enabled,
                u.DisplayName,
                u.GivenName,
                u.Surname,
                u.DistinguishedName.Replace(",DC=bank,DC=cibank,DC=ru", "")
            );

            if (!UserInfos.Contains(userInfo))
            {
                UserInfos.Add(userInfo);
            }
        }

        private void AddLocalUser(string principal)
        {
            string group, user;
            if (principal.Contains("\\"))
            {
                string[] account = principal.Split('\\');
                group = account[0];
                user = account[1];
            }
            else
            {
                group = "";
                user = principal;
            }

            var userInfo = new UserInfo(
                user,
                false,
                principal,
                user,
                group,
                Environment.MachineName + ", " + principal
            );

            if (!UserInfos.Contains(userInfo))
            {
                UserInfos.Add(userInfo);
            }
        }
    }
}
