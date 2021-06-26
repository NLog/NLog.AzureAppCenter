using System;
using System.Collections.Generic;
using Xunit;

namespace NLog.Targets.AppCenter.Tests
{
    public class StringDictionaryTests
    {
        [Fact]
        public void IndexOperatorTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            Assert.Throws<KeyNotFoundException>(() => stringDictionary["key"]);
            objectDictionary["key"] = "value";
            Assert.Equal("value", stringDictionary["key"]);
            stringDictionary["key"] = null;
            Assert.Null(stringDictionary["key"]);
        }

        [Fact]
        public void KeysCollectionTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            Assert.Empty(stringDictionary.Keys);
            objectDictionary["key"] = "value";
            Assert.Single(stringDictionary.Keys);
            Assert.Contains("key", stringDictionary.Keys);
        }

        [Fact]
        public void ValuesCollectionTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            Assert.Empty(stringDictionary.Values);
            objectDictionary["key"] = "value";
            Assert.Single(stringDictionary.Values);
            Assert.Contains("value", stringDictionary.Values);
        }

        [Fact]
        public void CountTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            var itemCount = stringDictionary.Count;
            Assert.Equal(0, itemCount);
            objectDictionary["key"] = "value";
            itemCount = stringDictionary.Count;
            Assert.Equal(1, itemCount);
        }

        [Fact]
        public void AddTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            stringDictionary.Add("key", "value");
            Assert.Single(stringDictionary);
            Assert.Contains(new KeyValuePair<string, string>("key", "value"), stringDictionary);
            Assert.Throws<ArgumentException>(() => stringDictionary.Add(new KeyValuePair<string, string>("key", "value")));
            stringDictionary.Add(new KeyValuePair<string, string>("value", "key"));
            Assert.Equal(2, stringDictionary.Count);
        }

        [Fact]
        public void RemoveTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            Assert.False(stringDictionary.Remove("key"));
            Assert.False(stringDictionary.Remove(new KeyValuePair<string, string>("value", "key")));
            objectDictionary["key"] = "value";
            Assert.True(stringDictionary.Remove("key"));
            Assert.Empty(stringDictionary);
        }

        [Fact]
        public void ContainsTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            Assert.False(stringDictionary.ContainsKey("key"));
            Assert.DoesNotContain(new KeyValuePair<string, string>("key", "value"), stringDictionary);
            objectDictionary["key"] = "value";
            Assert.True(stringDictionary.ContainsKey("key"));
            Assert.Contains(new KeyValuePair<string, string>("key", "value"), stringDictionary);
        }

        [Fact]
        public void EnumeratorTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            foreach (var item in stringDictionary)
                Assert.False(false);
            objectDictionary["key"] = 1;
            foreach (var item in stringDictionary)
            {
                Assert.Equal("key", item.Key);
                Assert.Equal("1", item.Value);
            }
        }

        [Fact]
        public void TryGetValueTest()
        {
            var objectDictionary = new Dictionary<string, object>();
            var stringDictionary = new StringDictionary(objectDictionary);
            Assert.False(stringDictionary.TryGetValue("key", out var noValue));
            objectDictionary["key"] = 1;
            Assert.True(stringDictionary.TryGetValue("key", out var itemValue));
            Assert.Equal("1", itemValue);
        }
    }
}
