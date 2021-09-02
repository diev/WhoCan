#region License
//------------------------------------------------------------------------------
// Copyright (c) 2020 Dmitrii Evdokimov
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
// (c) https://stackoverflow.com/questions/33923951/how-to-get-a-list-of-local-machine-groups-users-when-machine-is-not-in-active
//------------------------------------------------------------------------------
#endregion License

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace WhoCan
{
    public static class LocalGroups
    {
        public static IEnumerable<string> GetUserNames()
        {
            var buffer = IntPtr.Zero;

            try
            {
                uint entriesRead = 0;
                uint totalEntries = 0;

                var result = NativeMethods.NetUserEnum(null, 0, 0, ref buffer, uint.MaxValue, ref entriesRead, ref totalEntries, IntPtr.Zero);

                if (result != 0)
                {
                    throw new Win32Exception((int)result);
                }

                var userNames = Enumerable
                    .Range(0, (int)entriesRead)
                    .Select(i => {
                        var userInfo = Marshal.ReadIntPtr(buffer, i * IntPtr.Size);
                        var userName = Marshal.PtrToStringAuto(userInfo);

                        return userName;
                    })
                    .ToList();

                return userNames;
            }
            finally
            {
                NativeMethods.NetApiBufferFree(buffer);
            }
        }

        public static IEnumerable<string> GetLocalGroupNames()
        {
            var buffer = IntPtr.Zero;

            try
            {
                uint entriesRead = 0;
                uint totalEntries = 0;

                var result = NativeMethods.NetLocalGroupEnum(null, 0, ref buffer, uint.MaxValue, ref entriesRead, ref totalEntries, IntPtr.Zero);

                if (result != 0)
                {
                    throw new Win32Exception((int)result);
                }

                var localGroupNames = Enumerable
                    .Range(0, (int)entriesRead)
                    .Select(i => {
                        var localGroupInfo = Marshal.ReadIntPtr(buffer, i * IntPtr.Size);
                        var groupName = Marshal.PtrToStringAuto(localGroupInfo);
                        
                        return groupName;
                    })
                    .ToList();

                return localGroupNames;
            }
            finally
            {
                NativeMethods.NetApiBufferFree(buffer);
            }
        }

        public static IEnumerable<string> GetLocalGroupUsers(string localGroupName)
        {
            var buffer = IntPtr.Zero;

            try
            {
                uint entriesRead = 0;
                uint totalEntries = 0;

                var result = NativeMethods.NetLocalGroupGetMembers(null, localGroupName, 3, ref buffer, uint.MaxValue, ref entriesRead, ref totalEntries, IntPtr.Zero);

                if (result != 0)
                {
                    throw new Win32Exception((int)result);
                }

                var userNames = Enumerable
                    .Range(0, (int)entriesRead)
                    .Select(i => {
                        var membersInfo = Marshal.ReadIntPtr(buffer, i * IntPtr.Size);
                        var userName = Marshal.PtrToStringAuto(membersInfo);
                
                        return userName;
                    })
                    .ToList();

                return userNames;
            }
            finally
            {
                NativeMethods.NetApiBufferFree(buffer);
            }
        }
    }
}
