using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace CommonHelpers.Newstonsoft.Json.Tests
{
    public class JsonSerializedKeysDictionaryTests
    {
        private sealed class SampleKey : IEquatable<SampleKey>
        {
            public int X { get; set; }
            public string? Y { get; set; }

            public bool Equals(SampleKey? other) => other is not null &&
                X == other.X &&
                Y == other.Y;

            public override bool Equals(object? obj) => obj is SampleKey other && Equals(other);
            public override int GetHashCode() => (X, Y).GetHashCode();
        }

        [Fact]
        public void TestSpecializedDictionaryRoundtrippingViaJson()
        {
            var dict = new JsonSerializedKeysDictionary<SampleKey, string>();
            var key1 = new SampleKey { X = 1, Y = "abc" };
            var value1 = "qwerty";
            var key2 = new SampleKey { X = -5, Y = "qwe" };
            var value2 = "abcdef";

            dict.Add(key1, value1);
            dict.Add(key2, value2);

            var jsonConverter = new JsonSerializedKeysDictionaryJsonConverter<JsonSerializedKeysDictionary<SampleKey, string>, SampleKey, string>
                (capacity => new JsonSerializedKeysDictionary<SampleKey, string>(capacity));
            var json = JsonConvert.SerializeObject(dict, jsonConverter);
            var roundtrippedDict = JsonConvert.DeserializeObject<JsonSerializedKeysDictionary<SampleKey, string>>(json, jsonConverter);

            roundtrippedDict.Should().NotBeNullOrEmpty();
            roundtrippedDict.Should().NotBeSameAs(dict);
            roundtrippedDict.Should().HaveCount(dict.Count);
            roundtrippedDict.Should().OnlyContain(x => x.Key.Equals(key1) && x.Value.Equals(value1) || x.Key.Equals(key2) && x.Value.Equals(value2));
        }

        [Fact]
        public void TestRegularDictionaryRoundtrippingViaJson()
        {
            var dict = new Dictionary<SampleKey, string>();
            var key1 = new SampleKey { X = 1, Y = "abc" };
            var value1 = "qwerty";
            var key2 = new SampleKey { X = -5, Y = "qwe" };
            var value2 = "abcdef";

            dict.Add(key1, value1);
            dict.Add(key2, value2);

            var jsonConverter = new JsonSerializedKeysDictionaryJsonConverter<Dictionary<SampleKey, string>, SampleKey, string>
                (capacity => new Dictionary<SampleKey, string>(capacity));
            var json = JsonConvert.SerializeObject(dict, jsonConverter);
            var roundtrippedDict = JsonConvert.DeserializeObject<Dictionary<SampleKey, string>>(json, jsonConverter);

            roundtrippedDict.Should().NotBeNullOrEmpty();
            roundtrippedDict.Should().NotBeSameAs(dict);
            roundtrippedDict.Should().HaveCount(dict.Count);
            roundtrippedDict.Should().OnlyContain(x => x.Key.Equals(key1) && x.Value.Equals(value1) || x.Key.Equals(key2) && x.Value.Equals(value2));
        }

        [Fact]
        public void DictionaryWithStringKeysIsSerializedToJsonTheSameWayAsRegularDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                ["key1"] = "value1"
            };

            var jsonConverter = new JsonSerializedKeysDictionaryJsonConverter<Dictionary<string, string>, string, string>
                (capacity => new Dictionary<string, string>(capacity));
            var jsonWithConverter = JsonConvert.SerializeObject(dict, jsonConverter);
            var jsonWithoutConverter = JsonConvert.SerializeObject(dict);

            jsonWithConverter.Should().BeEquivalentTo(jsonWithoutConverter);
        }
    }
}