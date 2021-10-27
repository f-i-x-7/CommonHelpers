using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonHelpers.Newstonsoft.Json
{
    public sealed class JsonSerializedKeysDictionaryJsonConverter<TKey, TValue> : JsonConverter<JsonSerializedKeysDictionary<TKey, TValue>> where TKey : class
    {
        public override JsonSerializedKeysDictionary<TKey, TValue>? ReadJson(JsonReader reader, Type objectType, JsonSerializedKeysDictionary<TKey, TValue>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new ArgumentException($"{nameof(reader.TokenType)} expected to be {JsonToken.StartObject}, but was {reader.TokenType}.", nameof(reader));
            }

            var objectJson = JObject.Load(reader).ToString();
            var rawDictionary = JsonConvert.DeserializeObject<JsonSerializedKeysDictionary<string, TValue>>(objectJson);
            if (rawDictionary is null)
                return null;

            if (typeof(TKey) == typeof(string))
                return (JsonSerializedKeysDictionary<TKey, TValue>)(object)rawDictionary;

            var result = new JsonSerializedKeysDictionary<TKey, TValue>(rawDictionary.Count);

            foreach (var kvPair in rawDictionary)
            {
                var deserializedKey = JsonConvert.DeserializeObject<TKey>(kvPair.Key);
                if (deserializedKey is not null)
                {
                    result.Add(deserializedKey, kvPair.Value);
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, JsonSerializedKeysDictionary<TKey, TValue>? value, JsonSerializer serializer)
        {
            if (value is null)
                return;

            JObject jObject;

            if (typeof(TKey) == typeof(string))
            {
                jObject = JObject.FromObject(value);
            }
            else
            {
                var convertedValue = new Dictionary<string, TValue>(value.Count);
                foreach (var kvPair in value)
                {
                    convertedValue.Add(JsonConvert.SerializeObject(kvPair.Key), kvPair.Value);
                }

                jObject = JObject.FromObject(convertedValue);
            }

            jObject.WriteTo(writer);
        }
    }
}
