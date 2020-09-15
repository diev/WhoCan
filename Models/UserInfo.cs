﻿#region License
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
    public class UserInfo : BaseObject, IEquatable<UserInfo>
    {
        public UserInfo(string userName, bool enabled, string displayName, string name, string family, string comment)
        {
            UserName = userName;
            Enabled = enabled;
            DisplayName = displayName;
            Name = name;
            Family = family;
            Comment = comment;
        }

        public string UserName
        {
            get => GetValue<string>(nameof(UserName));
            private set => SetValue(nameof(UserName), value);
        }

        public bool Enabled
        {
            get => GetValue<bool>(nameof(Enabled));
            private set => SetValue(nameof(Enabled), value);
        }

        public string DisplayName
        {
            get => GetValue<string>(nameof(DisplayName));
            private set => SetValue(nameof(DisplayName), value);
        }

        public string Name
        {
            get => GetValue<string>(nameof(Name));
            private set => SetValue(nameof(Name), value);
        }

        public string Family
        {
            get => GetValue<string>(nameof(Family));
            private set => SetValue(nameof(Family), value);
        }

        public string Comment
        {
            get => GetValue<string>(nameof(Comment));
            private set => SetValue(nameof(Comment), value);
        }

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