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
using System.Runtime.InteropServices;

namespace WhoCan
{
    static class NativeMethods
    {
        [DllImport("netapi32.dll")]
        public static extern void NetApiBufferFree(IntPtr bufptr);

        [DllImport("netapi32.dll")]
        public static extern UInt32 NetUserEnum(
            [MarshalAs(UnmanagedType.LPWStr)] String servername, 
            UInt32 level, 
            UInt32 filter, 
            ref IntPtr bufptr, 
            UInt32 prefmaxlen, 
            ref UInt32 entriesread, 
            ref UInt32 totalentries, 
            IntPtr resumehandle);

        [DllImport("netapi32.dll")]
        public static extern UInt32 NetLocalGroupEnum(
            [MarshalAs(UnmanagedType.LPWStr)] String servername, 
            UInt32 level, 
            ref IntPtr bufptr, 
            UInt32 prefmaxlen, 
            ref UInt32 entriesread, 
            ref UInt32 totalentries, 
            IntPtr resumehandle);

        [DllImport("netapi32.dll")]
        public extern static UInt32 NetLocalGroupGetMembers(
            [MarshalAs(UnmanagedType.LPWStr)] String servername, 
            [MarshalAs(UnmanagedType.LPWStr)] String localgroupname, 
            UInt32 level, 
            ref IntPtr bufptr, 
            UInt32 prefmaxlen, 
            ref UInt32 entriesread, 
            ref UInt32 totalentries, 
            IntPtr resumehandle);
    }
}
