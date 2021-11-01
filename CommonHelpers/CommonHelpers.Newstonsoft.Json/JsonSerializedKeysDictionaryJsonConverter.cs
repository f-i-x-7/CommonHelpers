using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonHelpers.Newstonsoft.Json
{
    /// <summary>
    /// An implementation of <see cref="JsonConverter"/> that can be used for roundtrippable JSON serialization of objects implementing <see cref="IDictionary{TKey, TValue}"/> interface.
    /// </summary>
    /// <typeparam name="TDictionary">Dictionary type implementing <see cref="IDictionary{TKey, TValue}"/> interface.</typeparam>
    /// <typeparam name="TKey">Type of <typeparamref cref="TDictionary"/> key.</typeparam>
    /// <typeparam name="TValue">Type of <typeparamref name="TDictionary"/> value.</typeparam>
    /// <remarks>
    /// By default, Newtonsoft.Json serializes IDictionary keys of custom types via <see cref="object.ToString()"/> call (see https://www.newtonsoft.com/json/help/html/SerializationGuide.htm#Breakdown).
    /// This means that for custom types result JSON could not be deserialized back.<br/>
    /// This class serializes dictionary keys as JSON.
    /// </remarks>
    public class JsonSerializedKeysDictionaryJsonConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
        where TDictionary : class, IDictionary<TKey, TValue>
        where TKey : class // this constraint reduces number of special-cases where default Newtonsoft.Json serialization should be performed just to string keys
    {
        private readonly Func<TDictionary>? _factory;
        private readonly Func<int, TDictionary>? _factoryWithCapacity;

        public JsonSerializedKeysDictionaryJsonConverter()
        {
            var type = typeof(TDictionary);
            if (type.IsInterface)
            {
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    // TDictionary is IDictionary<TKey, TValue>. Try to use Dictionary<TKey, TValue>, even its capacity constructor.
                    _factoryWithCapacity = capacity => (TDictionary)(object)new Dictionary<TKey, TValue>(capacity);
                }
                else
                {
                    // Some more complicated interface. We cannot assume what type to use, and Activator.CreateInstance() will definitely fail later. So fail fast now.
                    var message = $"This constructor cannot be used when {nameof(TDictionary)} is an interface other than IDictionary<TKey, TValue>. {nameof(TDictionary)} used: '{typeof(TDictionary).FullName}'.";
                    throw new InvalidOperationException(message);
                }
            }
            else
            {
                _factory = () => Activator.CreateInstance<TDictionary>();
            }
        }

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
