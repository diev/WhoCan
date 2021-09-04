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
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;
using System.Text;

namespace WhoCan
{
    public static class Helpers
    {
        private static readonly PrincipalContext _domainContext;
        private static readonly PrincipalContext _machineContext;

        private static readonly List<string> _skipUsers = new List<string> {
            "admin",
            "Administrator",
            "Администратор"
        };

        private static readonly List<string> _skipGroups = new List<string> {
            "admins",
            "Администраторы домена",
            "Администраторы предприятия",
            "Все",
            "Высокий обязательный уровень",
            "Данная организация",
            "Подтвержденное службой удостоверение",
            "Пользователи домена",
            "Пользователи журналов производительности",
            "Пользователи удаленного рабочего стола",
            "Прошедшие проверку",
            "Средний обязательный уровень"
        };

        static Helpers()
        {
            try
            {
                _domainContext = new PrincipalContext(ContextType.Domain);
                _skipGroups.AddRange(new List<string>
                {
                    "Administrators",
                    "Администраторы",
                    "Users",
                    "Пользователи"
                });
            }
            catch { }

            _machineContext = new PrincipalContext(ContextType.Machine);
        }

        public static Principal FindByIdentity(string identityValue)
        {
            if (_domainContext != null)
            {
                Principal principal = Principal.FindByIdentity(_domainContext, identityValue);

                if (principal != null)
                {
                    return principal;
                }
            }
            
            return Principal.FindByIdentity(_machineContext, identityValue);
        }

        public static bool IsSystemName(bool isGroup, string name)
        {
            if (isGroup)
            {
                if (_skipGroups.Contains(name))
                {
                    return true;
                }
            }
            else
            {
                if (_skipUsers.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        public static string DistinguishedName(string distinguishedName)
        {
            // "CN=Name Surname,OU=Dept,OU=XXXX,DC=XXX,DC=XXXX,DC=XX" -> "Name Surname, Dept"

            if (distinguishedName == null)
            {
                return string.Empty;
            }

            int i = distinguishedName.IndexOf(",DC=");
            //i = distinguishedName.LastIndexOf(",OU=", i);
            distinguishedName = distinguishedName.Substring(0, i).Replace("CN=", string.Empty).Replace("OU=", " ");

            return distinguishedName;
        }

        public static string GetRightsEnum(FileSystemAccessRule rule, bool verbose = false)
        {
            var rights = new StringBuilder();

            if (rule.AccessControlType.HasFlag(AccessControlType.Deny))
            {
                _ = rights.Append("Deny ");
            }

            //Specifies the right to append data to the end of a file.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.AppendData))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.AppendData));
            }

            //Specifies the right to change the security and audit rules associated with a file or folder.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.ChangePermissions))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ChangePermissions));
            }

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
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.ExecuteFile))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ExecuteFile));
            }

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
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ReadAndExecute));
            }

            //Specifies the right to open and copy file system attributes from a folder or file.
            //For example, this value specifies the right to view the file creation or modified date.
            //This does not include the right to read data, extended file system attributes, or access and audit rules.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ReadAttributes));
            }

            //Specifies the right to open and copy a file or folder.
            //This does not include the right to read file system attributes,
            //extended file system attributes, or access and audit rules.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.ReadData))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ReadData));
            }

            //Specifies the right to open and copy extended file system attributes from a folder or file.
            //For example, this value specifies the right to view author and content information.
            //This does not include the right to read data, file system attributes, or access and audit rules.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.ReadExtendedAttributes))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ReadExtendedAttributes));
            }

            //Specifies the right to open and copy access and audit rules from a folder or file.
            //This does not include the right to read data, file system attributes,
            //and extended file system attributes.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.ReadPermissions))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.ReadPermissions));
            }

            //Specifies whether the application can wait for a file handle to synchronize
            //with the completion of an I/O operation.
            //This value is automatically set when allowing access and automatically excluded when denying access.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.Synchronize))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.Synchronize));
            }

            //Specifies the right to change the owner of a folder or file.
            //Note that owners of a resource have full access to that resource.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.TakeOwnership))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.TakeOwnership));
            }

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
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.WriteAttributes));
            }

            //Specifies the right to open and write to a file or folder.
            //This does not include the right to open and write file system attributes,
            //extended file system attributes, or access and audit rules.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.WriteData))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.WriteData));
            }

            //Specifies the right to open and write extended file system attributes to a folder or file.
            //This does not include the ability to write data, attributes, or access and audit rules.
            if (verbose && rule.FileSystemRights.HasFlag(FileSystemRights.WriteExtendedAttributes))
            {
                _ = rights.AppendLine(nameof(FileSystemRights.WriteExtendedAttributes));
            }

            string comment = rights.ToString().Replace(Environment.NewLine, ", ").Trim();

            if (comment.EndsWith(","))
            {
                comment = comment.Substring(0, comment.Length - 1);
            }

            if (rule.InheritanceFlags.Equals(InheritanceFlags.None))
            {
                comment = "[" + comment + "]";
            }

            if (string.IsNullOrEmpty(comment))
            {
                comment = rule.InheritanceFlags.ToString();
            }

            return comment;
        }
    }
}
