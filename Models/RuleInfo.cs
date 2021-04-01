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
        public RuleInfo(string principalName, bool domain, bool deny, string flags, string comment, bool group, bool danger)
        {
            PrincipalName = principalName;
            Domain = domain;
            Deny = deny;
            Flags = flags;
            Comment = comment;
            IsGroup = group;
            IsDanger = danger;
            IsSelected = false;
        }

        public string PrincipalName { get; set; }
        public bool Domain { get; set; }
        public bool Deny { get; set; }
        public string Flags { get; set; }
        public string Comment { get; set; }
        public bool IsGroup { get; set; }
        public bool IsDanger { get; set; }
        public bool IsSelected { get; set; }
    }
}
