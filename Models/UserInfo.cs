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

namespace WhoCan.Models
{
    public class UserInfo : BaseObject, IEquatable<UserInfo>
    {
        public UserPrincipal UserPrincipal { get; set; }

        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string Comment { get; set; }

        public bool Enabled { get; set; }
        public bool IsDanger { get; set; }
        public bool IsSelected { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserInfo);
        }

        public bool Equals(UserInfo other)
        {
            return other != null && UserName == other.UserName;
        }

        public override int GetHashCode()
        {
            return 404878561 + EqualityComparer<string>.Default.GetHashCode(UserName);
        }

        public static bool operator ==(UserInfo left, UserInfo right)
        {
            return EqualityComparer<UserInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(UserInfo left, UserInfo right)
        {
            return !(left == right);
        }
    }
}
