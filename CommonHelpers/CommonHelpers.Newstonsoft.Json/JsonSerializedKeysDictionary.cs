using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CommonHelpers.Newstonsoft.Json
{
    public sealed class JsonSerializedKeysDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public JsonSerializedKeysDictionary() : base() { }

        public JsonSerializedKeysDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        public JsonSerializedKeysDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

        public JsonSerializedKeysDictionary(int capacity) : base(capacity) { }

        public JsonSerializedKeysDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

        public JsonSerializedKeysDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        private JsonSerializedKeysDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
