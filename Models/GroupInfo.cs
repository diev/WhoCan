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
using System.Collections.Generic;

namespace WhoCan.Models
{
    public class GroupInfo : BaseObject, IEquatable<GroupInfo>
    {
        public GroupInfo(string groupName, string description)
        {
            GroupName = groupName;
            Description = description;
        }

        public string GroupName
        {
            get => GetValue<string>(nameof(GroupName));
            private set => SetValue(nameof(GroupName), value);
        }

        public string Description
        {
            get => GetValue<string>(nameof(Description));
            private set => SetValue(nameof(Description), value);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupInfo);
        }

        public bool Equals(GroupInfo other)
        {
            return other != null && GroupName == other.GroupName;
        }

        public override int GetHashCode()
        {
            return 404879561 + EqualityComparer<string>.Default.GetHashCode(GroupName);
        }

        public static bool operator ==(GroupInfo left, GroupInfo right)
        {
            return EqualityComparer<GroupInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(GroupInfo left, GroupInfo right)
        {
            return !(left == right);
        }
    }
}
