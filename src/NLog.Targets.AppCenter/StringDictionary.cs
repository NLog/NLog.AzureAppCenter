// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections;
using System.Collections.Generic;

namespace NLog.Targets
{
    /// <summary>
    /// Wrapper for object dictionary to reduce allocations
    /// </summary>
    internal class StringDictionary : IDictionary<string, string>
    {
        private readonly IDictionary<string, object?> _dictionary;

        public StringDictionary(IDictionary<string, object?> dictionary)
        {
            _dictionary = dictionary;
        }

        public string this[string key]
        {
            get => ConvertToString(_dictionary[key]);
            set => _dictionary[key] = value;
        }

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<string> Values
        {
            get
            {
                var valueList = new List<string>(Count);
                foreach (var item in _dictionary)
                    valueList.Add(ConvertToString(item.Value));
                return valueList;
            }
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public void Add(string key, string value)
        {
            _dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            _dictionary.Add(new KeyValuePair<string, object?>(item.Key, item.Value));
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _dictionary.Contains(new KeyValuePair<string, object?>(item.Key, item.Value));
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (Count > 0)
            {
                foreach (var item in _dictionary)
                {
                    array[arrayIndex++] = new KeyValuePair<string, string>(item.Key, ConvertToString(item.Value));
                }
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var item in _dictionary)
                yield return new KeyValuePair<string, string>(item.Key, ConvertToString(item.Value));
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _dictionary.Contains(new KeyValuePair<string, object?>(item.Key, item.Value));
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_dictionary.TryGetValue(key, out var objectValue))
            {
                value = ConvertToString(objectValue);
                return true;
            }
            value = string.Empty;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private string ConvertToString(object? value)
        {
            try
            {
                return value?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
