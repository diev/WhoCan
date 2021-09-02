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
using System.ComponentModel;

namespace TreeViewFileExplorer.ShellClasses
{
    [Serializable]
    public abstract class BaseFileSystemObjectInfo : INotifyPropertyChanged
    {
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        protected BaseFileSystemObjectInfo() : base() { }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public T GetValue<T>(string key)
        {
            return (GetValue(key) is T t) ? t : default;
        }

        private object GetValue(string key)
        {
            return _values.TryGetValue(key, out object stored) ? stored : null;
        }

        public void SetValue(string key, object value)
        {
            if (_values.TryGetValue(key, out object stored))
            {
                if (value.Equals(stored))
                {
                    return;
                }

                _values[key] = value;
            }
            else
            {
                _values.Add(key, value);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }
    }
}
