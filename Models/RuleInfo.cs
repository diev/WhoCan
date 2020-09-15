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

namespace WhoCan.Models
{
    public class RuleInfo : BaseObject
    {
        public RuleInfo(string principalName, bool domain, bool deny, string flags, string comment)
        {
            PrincipalName = principalName;
            Domain = domain;
            Deny = deny;
            Flags = flags;
            Comment = comment;
            IsSelected = false;
        }

        public string PrincipalName
        {
            get => GetValue<string>(nameof(PrincipalName));
            set => SetValue(nameof(PrincipalName), value);
        }

        public bool Domain
        {
            get => GetValue<bool>(nameof(Domain));
            set => SetValue(nameof(Domain), value);
        }

        public bool Deny
        {
            get => GetValue<bool>(nameof(Deny));
            set => SetValue(nameof(Deny), value);
        }

        public string Flags
        {
            get => GetValue<string>(nameof(Flags));
            set => SetValue(nameof(Flags), value);
        }

        public string Comment
        {
            get => GetValue<string>(nameof(Comment));
            set => SetValue(nameof(Comment), value);
        }
        public bool IsSelected
        {
            get => GetValue<bool>(nameof(IsSelected));
            set => SetValue(nameof(IsSelected), value);
        }
    }
}
