using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonHelpers.Newstonsoft.Json
{
    public class JsonSerializedKeysDictionaryJsonConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
        where TDictionary : class, IDictionary<TKey, TValue>
        where TKey : class
    {
        private readonly Func<TDictionary>? _factory;
        private readonly Func<int, TDictionary>? _factoryWithCapacity;

        public JsonSerializedKeysDictionaryJsonConverter(Func<TDictionary> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public JsonSerializedKeysDictionaryJsonConverter(Func<int, TDictionary> factoryWithCapacity)
        {
            _factoryWithCapacity = factoryWithCapacity ?? throw new ArgumentNullException(nameof(factoryWithCapacity));
        }

        public override TDictionary? ReadJson(JsonReader reader, Type objectType, TDictionary? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new ArgumentException($"{nameof(reader.TokenType)} expected to be {JsonToken.StartObject}, but was {reader.TokenType}.", nameof(reader));
            }

            var objectJson = JObject.Load(reader).ToString();
            var rawDictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(objectJson);
            if (rawDictionary is null)
                return null;

            if (typeof(TKey) == typeof(string))
                return (TDictionary)(object)rawDictionary;

            TDictionary result;
            if (_factoryWithCapacity is not null)
            {
                result = _factoryWithCapacity(rawDictionary.Count);
            }
            else
            {
                Debug.Assert(_factory is not null);
                result = _factory!();
            }

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

        public override void WriteJson(JsonWriter writer, TDictionary? value, JsonSerializer serializer)
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
