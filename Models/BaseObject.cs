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
    [Serializable]
    public abstract class BaseObject : PropertyNotifier
    {
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public T GetValue<T>(string key)
        {
            var value = GetValue(key);
            return (value is T t) ? t : default;
        }

        private object GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return _values.ContainsKey(key) ? _values[key] : null;
        }

        public void SetValue(string key, object value)
        {
            if (!_values.ContainsKey(key))
            {
                _values.Add(key, value);
            }
            else
            {
                _values[key] = value;
            }

            OnPropertyChanged(key);
        }
    }
}
