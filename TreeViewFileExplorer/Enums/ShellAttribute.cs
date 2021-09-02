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
// (c) https://github.com/mikependon/Tutorials/tree/master/WPF/TreeViewFileExplorer
//------------------------------------------------------------------------------
#endregion License

using System;

namespace TreeViewFileExplorer.Enums
{
    [Flags]
    public enum ShellAttribute : uint
    {
        LargeIcon = 0,              // 0x000000000
        SmallIcon = 1,              // 0x000000001
        OpenIcon = 2,               // 0x000000002
        ShellIconSize = 4,          // 0x000000004
        Pidl = 8,                   // 0x000000008
        UseFileAttributes = 16,     // 0x000000010
        AddOverlays = 32,           // 0x000000020
        OverlayIndex = 64,          // 0x000000040
        Others = 128,               // Not defined, really?
        Icon = 256,                 // 0x000000100  
        DisplayName = 512,          // 0x000000200
        TypeName = 1024,            // 0x000000400
        Attributes = 2048,          // 0x000000800
        IconLocation = 4096,        // 0x000001000
        ExeType = 8192,             // 0x000002000
        SystemIconIndex = 16384,    // 0x000004000
        LinkOverlay = 32768,        // 0x000008000 
        Selected = 65536,           // 0x000010000
        AttributeSpecified = 131072 // 0x000020000
    }
}
