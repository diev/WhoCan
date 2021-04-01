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
        private const string CommonNT = @"BANK\";
        private const string CommonOU = ",OU=БАНК";
        private const string CommonDC = ",DC=bank,DC=cibank,DC=ru";

        private readonly string[] SkipAdmins = { 
            CommonNT + "admin", 
            @"BUILTIN\Администраторы", @"BUILTIN\Administrators",
            @"NT AUTHORITY\СИСТЕМА", @"NT AUTHORITY\SYSTEM"
        };

        private readonly string[] SkipGroups = {
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

        private const string RightDeny = "x";
        private const string RightFull = "F";
        private const string RightRead = "R";
        private const string RightWrite = "W";

        public ObservableCollection<RuleInfo> RuleInfos { get; set; }
        public ObservableCollection<UserInfo> UserInfos { get; set; }
        public ObservableCollection<GroupInfo> GroupInfos { get; set; }

        public MainViewModel()
        {
            RuleInfos = new ObservableCollection<RuleInfo>();
            UserInfos = new ObservableCollection<UserInfo>();
            GroupInfos = new ObservableCollection<GroupInfo>();
        }

        public FileSystemObjectInfo SelectedFileSystemObjectInfo { get; set; }
        public UserInfo SelectedUserInfo { get; set; }
        public GroupInfo SelectedGroupInfo { get; set; }

        public ObservableCollection<RuleInfo> GetRuleInfos()
        {
            RuleInfos.Clear();

            PrincipalContext principalContext = null;
            try { principalContext = new PrincipalContext(ContextType.Domain); } catch { }

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
                    var name = rule.IdentityReference.Value;
                    bool group = true;

                    if (principalContext != null)
                    {
                        bool skip = false;

                        foreach (string admin in SkipAdmins)
                        {
                            if (name.Equals(admin))
                            {
                                skip = true;
                                break;
                            }
                        }

                        if (skip)
                        {
                            continue;
                        }

                        var principal = Principal.FindByIdentity(principalContext, name);
                        group = principal is GroupPrincipal;
                    }

                    bool deny = rule.AccessControlType.HasFlag(AccessControlType.Deny);
                    var flags = new StringBuilder();
                    string comment;
                    bool danger = false;

                    if (deny)
                    {
                        flags.Append(RightDeny);

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Write | FileSystemRights.Delete))
                        {
                            flags.Append(RightWrite);
                        }

                        comment = "Deny " + rule.FileSystemRights.ToString();
                    }
                    else
                    {
                        if (rule.FileSystemRights.HasFlag(FileSystemRights.FullControl))
                        {
                            danger = true;
                            flags.Append(RightFull);
                        }

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute))
                        {
                            flags.Append(RightRead);
                        }

                        if (rule.FileSystemRights.HasFlag(FileSystemRights.Modify | FileSystemRights.Delete))
                        {
                            danger = true;
                            flags.Append(RightWrite);
                        }

                        comment = rule.FileSystemRights.ToString();
                    }

                    if (principalContext != null)
                    {
                        var principal = Principal.FindByIdentity(principalContext, name);
                        group = principal is GroupPrincipal;

                        RuleInfos.Add(new RuleInfo(
                            name.Replace(CommonNT, string.Empty),
                            name.StartsWith(CommonNT),
                            deny,
                            flags.ToString(),
                            comment,
                            group,
                            danger)
                        );
                    }
                    else
                    {
                        RuleInfos.Add(new RuleInfo(
                            name,
                            false,
                            deny,
                            flags.ToString(),
                            comment,
                            group,
                            danger)
                        );
                    }
                }
            }
            catch { }

            return RuleInfos;
        }

        public ObservableCollection<UserInfo> GetUserInfos()
        {
            UserInfos.Clear();

            PrincipalContext principalContext = null;
            try { principalContext = new PrincipalContext(ContextType.Domain); } catch { }

            if (principalContext != null)
            {
                foreach (var ruleInfo in RuleInfos)
                {
                    if (ruleInfo.IsSelected)
                    {
                        AddUser(principalContext, ruleInfo);
                    }
                }
            }
            else
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

        public ObservableCollection<UserInfo> GetAllUserInfos()
        {
            UserInfos.Clear();

            PrincipalContext principalContext = null;
            try { principalContext = new PrincipalContext(ContextType.Domain); } catch { }

            if (principalContext != null)
            {
                foreach (var ruleInfo in RuleInfos)
                {
                    AddUser(principalContext, ruleInfo);
                }
            }
            else
            {
                foreach (var ruleInfo in RuleInfos)
                {
                    AddLocalUser(ruleInfo.PrincipalName);
                }
            }

            return UserInfos;
        }

        private void AddUser(PrincipalContext principalContext, RuleInfo ruleInfo)
        {
            var principal = Principal.FindByIdentity(principalContext, ruleInfo.PrincipalName);

            if (principal is GroupPrincipal group)
            {
                var members = group.GetMembers(true);

                foreach (Principal member in members)
                {
                    if (member is UserPrincipal user1)
                    {
                        AddDomainUser(user1, ruleInfo);
                    }
                }
            }

            if (principal is UserPrincipal user)
            {
                AddDomainUser(user, ruleInfo);
            }
        }

        private void AddDomainUser(UserPrincipal user, RuleInfo ruleInfo)
        {
            var userInfo = new UserInfo(
                user.SamAccountName, //.UserPrincipalName,
                (bool)user.Enabled,
                user.DisplayName,
                user.GivenName,
                user.Surname,
                user.DistinguishedName // "CN=Name Surname,OU=Dept,OU=XXXX,DC=XXX,DC=XXXX,DC=XX" -> "Name Surname, Dept"
                    .Replace("CN=", string.Empty)
                    .Replace(CommonOU, string.Empty)
                    .Replace(CommonDC, string.Empty)
                    .Replace("OU=", " "),
                (bool)user.Enabled && ruleInfo.IsDanger
            );

            if (UserInfos.Contains(userInfo))
            {
                foreach (var userInfoIn in UserInfos)
                {
                    if (userInfoIn == userInfo)
                    {
                        if (userInfoIn.Enabled && !userInfoIn.IsDanger && userInfo.IsDanger)
                        {
                            userInfoIn.IsDanger = true;
                            break;
                        }
                    }
                }
            }
            else // add
            {
                if (!ruleInfo.Deny)
                {
                    UserInfos.Add(userInfo);
                }
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
                group = string.Empty;
                user = principal;
            }

            var userInfo = new UserInfo(
                user,
                false,
                principal,
                user,
                group,
                Environment.MachineName + ", " + principal,
                false
            );

            if (!UserInfos.Contains(userInfo))
            {
                UserInfos.Add(userInfo);
            }
        }

        public ObservableCollection<GroupInfo> GetGroupInfos(string userName)
        {
            GroupInfos.Clear();

            // establish domain context
            PrincipalContext principalContext = null;
            try { principalContext = new PrincipalContext(ContextType.Domain); } catch { }

            if (principalContext != null)
            {
                // find the user
                var user = UserPrincipal.FindByIdentity(principalContext, userName);

                // if found - grab its groups
                if (user != null)
                {
                    PrincipalSearchResult<Principal> groups = user.GetAuthorizationGroups();

                    // iterate over all groups
                    foreach (Principal p in groups)
                    {
                        // make sure to add only group principals
                        if (p is GroupPrincipal)
                        {
                            string name = p.Name;
                            bool skip = false;

                            foreach (string group in SkipGroups)
                            {
                                if (name.Equals(group))
                                {
                                    skip = true;
                                    break;
                                }
                            }

                            if (skip)
                            {
                                continue;
                            }

                            var groupInfo = new GroupInfo(
                                name,
                                p.Description
                            );

                            GroupInfos.Add(groupInfo);
                        }
                    }
                }
            }

            return GroupInfos;
        }
    }
}
