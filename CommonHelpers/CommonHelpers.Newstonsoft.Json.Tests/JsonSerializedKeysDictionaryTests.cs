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
        public void TestDictionaryRoundtrippingViaJson()
        {
            var dict = new JsonSerializedKeysDictionary<SampleKey, string>();
            var key1 = new SampleKey { X = 1, Y = "abc" };
            var value1 = "qwerty";
            var key2 = new SampleKey { X = -5, Y = "qwe" };
            var value2 = "abcdef";

            dict.Add(key1, value1);
            dict.Add(key2, value2);

            var jsonConverter = new JsonSerializedKeysDictionaryJsonConverter<SampleKey, string>();
            var json = JsonConvert.SerializeObject(dict, jsonConverter);
            var roundtrippedDict = JsonConvert.DeserializeObject<JsonSerializedKeysDictionary<SampleKey, string>>(json, jsonConverter);

            roundtrippedDict.Should().NotBeNullOrEmpty();
            roundtrippedDict.Should().NotBeSameAs(dict);
            roundtrippedDict.Should().HaveCount(dict.Count);
            roundtrippedDict.Should().OnlyContain(x => x.Key.Equals(key1) && x.Value.Equals(value1) || x.Key.Equals(key2) && x.Value.Equals(value2));
        }

        [Fact]
        public void DictionaryWithStringKeysIsSerializedToJsonTheSameWayAsRegularDictionary()
        {
            var dict = new JsonSerializedKeysDictionary<string, string>
            {
                ["key1"] = "value1"
            };

            var regularDict = new Dictionary<string, string>
            {
                ["key1"] = "value1"
            };

            var json = JsonConvert.SerializeObject(dict, new JsonSerializedKeysDictionaryJsonConverter<string, string>());
            var regularJson = JsonConvert.SerializeObject(regularDict);

            json.Should().BeEquivalentTo(regularJson);
        }
    }
}