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
using System.Runtime.InteropServices;

namespace WhoCan
{
    internal static class NativeMethods
    {
        [DllImport("netapi32.dll")]
        public static extern void NetApiBufferFree(IntPtr bufptr);

        [DllImport("netapi32.dll")]
        public static extern uint NetUserEnum(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            uint level,
            uint filter,
            ref IntPtr bufptr,
            uint prefmaxlen,
            ref uint entriesread,
            ref uint totalentries,
            IntPtr resumehandle);

        [DllImport("netapi32.dll")]
        public static extern uint NetLocalGroupEnum(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            uint level,
            ref IntPtr bufptr,
            uint prefmaxlen,
            ref uint entriesread,
            ref uint totalentries,
            IntPtr resumehandle);

        [DllImport("netapi32.dll")]
        public extern static uint NetLocalGroupGetMembers(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            [MarshalAs(UnmanagedType.LPWStr)] string localgroupname,
            uint level,
            ref IntPtr bufptr,
            uint prefmaxlen,
            ref uint entriesread,
            ref uint totalentries,
            IntPtr resumehandle);
    }
}
