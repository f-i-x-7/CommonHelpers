using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace CommonHelpers.Newstonsoft.Json.Tests
{
    public class JsonSerializedKeysDictionaryJsonConverterTests
    {
        #region Nested classes

        public enum ConstructorType
        {
            Paremeterless,
            FactoryWithoutCapacity,
            FactoryWithCapacity
        }

        public enum GenericArgumentType
        {
            Class,
            Interface
        }

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

        private sealed class SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass
        {
            public ConcurrentDictionary<SampleKey, string> DictDeclaredAsClass { get; set; } = new();
        }

        private sealed class SampleClassWithConcurrentDictionaryPropertyDeclaredAsInterface
        {
            public IDictionary<SampleKey, string> DictDeclaredAsInterface { get; set; } = new ConcurrentDictionary<SampleKey, string>();
        }

        private sealed class SampleClassWithDictionaryPropertiesDeclaredAsClassAndInterface
        {
            public Dictionary<SampleKey, string> DictDeclaredAsClass { get; set; } = new();
            public IDictionary<SampleKey, string> DictDeclaredAsInterface { get; set; } = new Dictionary<SampleKey, string>();
        }

        private sealed class SampleClassWithDictionaryFieldsDeclaredAsClassAndInterface
        {
            public Dictionary<SampleKey, string> DictDeclaredAsClass = new();
            public IDictionary<SampleKey, string> DictDeclaredAsInterface = new Dictionary<SampleKey, string>();
        }

        private sealed class SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface
        {
            public ConcurrentDictionary<SampleKey, string> DictDeclaredAsClass { get; set; } = new();
            public IDictionary<SampleKey, string> DictDeclaredAsInterface { get; set; } = new ConcurrentDictionary<SampleKey, string>();
        }

        private sealed class SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface
        {
            public ConcurrentDictionary<SampleKey, string> DictDeclaredAsClass = new();
            public IDictionary<SampleKey, string> DictDeclaredAsInterface = new ConcurrentDictionary<SampleKey, string>();
        }

        #endregion

        #region Test data

        public static IEnumerable<object[]> GetConstructorTypeAllValues()
        {
            yield return new object[] { ConstructorType.Paremeterless };
            yield return new object[] { ConstructorType.FactoryWithoutCapacity };
            yield return new object[] { ConstructorType.FactoryWithCapacity };
        }

        public static IEnumerable<object[]> GetConstructorTypeAndGenericArgumentTypeAllValues()
        {
            yield return new object[] { ConstructorType.Paremeterless, GenericArgumentType.Class };
            yield return new object[] { ConstructorType.FactoryWithoutCapacity, GenericArgumentType.Class };
            yield return new object[] { ConstructorType.FactoryWithCapacity, GenericArgumentType.Class };

            yield return new object[] { ConstructorType.Paremeterless, GenericArgumentType.Interface };
            yield return new object[] { ConstructorType.FactoryWithoutCapacity, GenericArgumentType.Interface };
            yield return new object[] { ConstructorType.FactoryWithCapacity, GenericArgumentType.Interface };
        }

        public static IEnumerable<object[]> GetConstructorTypeAndGenericArgumentTypeAllValues_ExceptParameterslessConstructorAndInterfaceGenericArgument()
        {
            yield return new object[] { ConstructorType.Paremeterless, GenericArgumentType.Class };
            yield return new object[] { ConstructorType.FactoryWithoutCapacity, GenericArgumentType.Class };
            yield return new object[] { ConstructorType.FactoryWithCapacity, GenericArgumentType.Class };

            yield return new object[] { ConstructorType.FactoryWithoutCapacity, GenericArgumentType.Interface };
            yield return new object[] { ConstructorType.FactoryWithCapacity, GenericArgumentType.Interface };
        }

        public static IEnumerable<object[]> GetConstructorTypeAndGenericArgumentTypeAllValues_OnlyParameterslessConstructorAndInterfaceGenericArgument()
        {
            yield return new object[] { ConstructorType.Paremeterless, GenericArgumentType.Interface };
        }

        #endregion

        #region Tests

        #region Dictionaries only

        [MemberData(nameof(GetConstructorTypeAllValues))]
        [Theory]
        public void RoundtrippingViaJson_For_SpecializedDictionary_Succeeds(ConstructorType constructorType)
        {
            var dict = new JsonSerializedKeysDictionary<SampleKey, string>();
            dict.Add(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            dict.Add(new SampleKey { X = -5, Y = "qwe" }, "abcdef");

            var jsonConverter = CreateJsonConverter<JsonSerializedKeysDictionary<SampleKey, string>, SampleKey, string>(constructorType,
                () => new JsonSerializedKeysDictionary<SampleKey, string>(),
                capacity => new JsonSerializedKeysDictionary<SampleKey, string>(capacity));
            var json = JsonConvert.SerializeObject(dict, jsonConverter);
            var roundtrippedDict = JsonConvert.DeserializeObject<JsonSerializedKeysDictionary<SampleKey, string>>(json, jsonConverter);

            AssertDictionary(dict, roundtrippedDict);
        }

        [MemberData(nameof(GetConstructorTypeAllValues))]
        [Theory]
        public void RoundtrippingViaJson_For_RegularDictionary_Succeeds(ConstructorType constructorType)
        {
            var dict = new Dictionary<SampleKey, string>();
            dict.Add(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            dict.Add(new SampleKey { X = -5, Y = "qwe" }, "abcdef");

            var jsonConverter = CreateJsonConverter<Dictionary<SampleKey, string>, SampleKey, string>(constructorType,
                DictionaryFactory, DictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(dict, jsonConverter);
            var roundtrippedDict = JsonConvert.DeserializeObject<Dictionary<SampleKey, string>>(json, jsonConverter);

            AssertDictionary(dict, roundtrippedDict);
        }

        [MemberData(nameof(GetConstructorTypeAllValues))]
        [Theory]
        public void RoundtrippingViaJson_For_ConcurrentDictionary_Succeeds(ConstructorType constructorType)
        {
            var dict = new ConcurrentDictionary<SampleKey, string>();
            dict.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            dict.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");

            var jsonConverter = CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType,
                ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(dict, jsonConverter);
            var roundtrippedDict = JsonConvert.DeserializeObject<ConcurrentDictionary<SampleKey, string>>(json, jsonConverter);

            AssertDictionary(dict, roundtrippedDict);
        }

        [MemberData(nameof(GetConstructorTypeAllValues))]
        [Theory]
        public void SerializationResult_WhenDictionaryWithStringKeysIsUsed_IsTheSameAsWhenConverterIsNotUsed(ConstructorType constructorType)
        {
            var dict = new Dictionary<string, string>
            {
                ["key1"] = "value1"
            };

            var jsonConverter = CreateJsonConverter<Dictionary<string, string>, string, string>(constructorType,
                () => new Dictionary<string, string>(),
                capacity => new Dictionary<string, string>(capacity));
            var jsonWithConverter = JsonConvert.SerializeObject(dict, jsonConverter);
            var jsonWithoutConverter = JsonConvert.SerializeObject(dict);

            jsonWithConverter.Should().BeEquivalentTo(jsonWithoutConverter);
        }

        #endregion

        #region Other

        #region DTO has only class or interface, single JsonConverter is used

        #region SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues_ExceptParameterslessConstructorAndInterfaceGenericArgument))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass_WithOnlyJsonConverterForClassOrInterface_Succeeds(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass();

            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);
            var roundtrippedObj = JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass>(json, jsonConverter);

            AssertDictionary(obj.DictDeclaredAsClass, roundtrippedObj?.DictDeclaredAsClass);
        }

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues_OnlyParameterslessConstructorAndInterfaceGenericArgument))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass_WithOnlyJsonConverterForClassOrInterface_FailsOnDeserialization(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass();

            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);

            var exception = Record.Exception(() => JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryPropertyDeclaredAsClass>(json, jsonConverter));
            exception.Should().NotBeNull();
        }

        #endregion

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryPropertyDeclaredAsInterface_WithOnlyJsonConverterForClassOrInterface_Succeeds(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithConcurrentDictionaryPropertyDeclaredAsInterface();

            obj.DictDeclaredAsInterface.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsInterface.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);
            var roundtrippedObj = JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryPropertyDeclaredAsInterface>(json, jsonConverter);

            AssertDictionary(obj.DictDeclaredAsInterface, roundtrippedObj?.DictDeclaredAsInterface);
        }

        #endregion

        #region Class and interface together in DTO, single JsonConverter is used

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithDictionaryPropertiesDeclaredAsClassAndInterface_WithOnlyJsonConverterForClassOrInterface_Succeeds(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithDictionaryPropertiesDeclaredAsClassAndInterface();

            obj.DictDeclaredAsClass.Add(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.Add(new SampleKey { X = -5, Y = "qwe" }, "abcdef");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 13, Y = "def" }, "uvw");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 42, Y = "qty" }, "xyz");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<Dictionary<SampleKey, string>, SampleKey, string>(constructorType, DictionaryFactory, DictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, DictionaryFactory, DictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);
            var roundtrippedObj = JsonConvert.DeserializeObject<SampleClassWithDictionaryPropertiesDeclaredAsClassAndInterface>(json, jsonConverter);

            AssertDictionary(obj.DictDeclaredAsClass, roundtrippedObj?.DictDeclaredAsClass);
            AssertDictionary(obj.DictDeclaredAsInterface, roundtrippedObj?.DictDeclaredAsInterface);
        }

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithDictionaryFieldsDeclaredAsClassAndInterface_WithOnlyJsonConverterForClassOrInterface_Succeeds(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithDictionaryFieldsDeclaredAsClassAndInterface();

            obj.DictDeclaredAsClass.Add(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.Add(new SampleKey { X = -5, Y = "qwe" }, "abcdef");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 13, Y = "def" }, "uvw");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 42, Y = "qty" }, "xyz");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<Dictionary<SampleKey, string>, SampleKey, string>(constructorType, DictionaryFactory, DictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, DictionaryFactory, DictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);
            var roundtrippedObj = JsonConvert.DeserializeObject<SampleClassWithDictionaryFieldsDeclaredAsClassAndInterface>(json, jsonConverter);

            AssertDictionary(obj.DictDeclaredAsClass, roundtrippedObj?.DictDeclaredAsClass);
            AssertDictionary(obj.DictDeclaredAsInterface, roundtrippedObj?.DictDeclaredAsInterface);
        }

        #region SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues_ExceptParameterslessConstructorAndInterfaceGenericArgument))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface_WithOnlyJsonConverterForClassOrInterface_Succeeds(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface();

            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 13, Y = "def" }, "uvw");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 42, Y = "qty" }, "xyz");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);
            var roundtrippedObj = JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface>(json, jsonConverter);

            AssertDictionary(obj.DictDeclaredAsClass, roundtrippedObj?.DictDeclaredAsClass);
            AssertDictionary(obj.DictDeclaredAsInterface, roundtrippedObj?.DictDeclaredAsInterface);
        }

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues_OnlyParameterslessConstructorAndInterfaceGenericArgument))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface_WithOnlyJsonConverterForClassOrInterface_FailsOnDeserialization(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface();

            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 13, Y = "def" }, "uvw");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 42, Y = "qty" }, "xyz");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);

            var exception = Record.Exception(() => JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface>(json, jsonConverter));
            exception.Should().NotBeNull();
        }

        #endregion

        #region SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues_ExceptParameterslessConstructorAndInterfaceGenericArgument))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface_WithOnlyJsonConverterForClassOrInterface_Succeeds(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface();

            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 13, Y = "def" }, "uvw");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 42, Y = "qty" }, "xyz");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);
            var roundtrippedObj = JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface>(json, jsonConverter);

            AssertDictionary(obj.DictDeclaredAsClass, roundtrippedObj?.DictDeclaredAsClass);
            AssertDictionary(obj.DictDeclaredAsInterface, roundtrippedObj?.DictDeclaredAsInterface);
        }

        [MemberData(nameof(GetConstructorTypeAndGenericArgumentTypeAllValues_OnlyParameterslessConstructorAndInterfaceGenericArgument))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface_WithOnlyJsonConverterForClassOrInterface_FailsOnDeserialization(ConstructorType constructorType,
            GenericArgumentType genericArgumentType)
        {
            var obj = new SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface();

            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 13, Y = "def" }, "uvw");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 42, Y = "qty" }, "xyz");

            JsonConverter jsonConverter = genericArgumentType == GenericArgumentType.Class
                ? CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity)
                : CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter);

            var exception = Record.Exception(() => JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryFieldsDeclaredAsClassAndInterface>(json, jsonConverter));
            exception.Should().NotBeNull();
        }

        #endregion

        #endregion

        #region Class and interface together in DTO, two JsonConverters are used

        [MemberData(nameof(GetConstructorTypeAllValues))]
        [Theory]
        public void RoundtrippingViaJson_For_SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface_WithTwoJsonConverterForClassOrInterface_Succeeds(ConstructorType constructorType)
        {
            var obj = new SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface();

            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = 1, Y = "abc" }, "qwerty");
            obj.DictDeclaredAsClass.TryAdd(new SampleKey { X = -5, Y = "qwe" }, "abcdef");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 13, Y = "def" }, "uvw");
            obj.DictDeclaredAsInterface.Add(new SampleKey { X = 42, Y = "qty" }, "xyz");

            JsonConverter jsonConverter_Class = CreateJsonConverter<ConcurrentDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            JsonConverter jsonConverter_Interface = CreateJsonConverter<IDictionary<SampleKey, string>, SampleKey, string>(constructorType, ConcurrentDictionaryFactory, ConcurrentDictionaryFactoryWithCapacity);
            var json = JsonConvert.SerializeObject(obj, jsonConverter_Class, jsonConverter_Interface);
            var roundtrippedObj = JsonConvert.DeserializeObject<SampleClassWithConcurrentDictionaryPropertiesDeclaredAsClassAndInterface>(json, jsonConverter_Class, jsonConverter_Interface);

            AssertDictionary(obj.DictDeclaredAsClass, roundtrippedObj?.DictDeclaredAsClass);
            AssertDictionary(obj.DictDeclaredAsInterface, roundtrippedObj?.DictDeclaredAsInterface);
        }

        #endregion

        #endregion

        #endregion

        private static JsonSerializedKeysDictionaryJsonConverter<TDictionary, TKey, TValue> CreateJsonConverter<TDictionary, TKey, TValue>(ConstructorType constructorType,
                Func<TDictionary> factory,
                Func<int, TDictionary> factoryWithCapacity)
            where TDictionary : class, IDictionary<TKey, TValue>
            where TKey : class
        {
            return constructorType switch
            {
                ConstructorType.Paremeterless => new JsonSerializedKeysDictionaryJsonConverter<TDictionary, TKey, TValue>(),
                ConstructorType.FactoryWithoutCapacity => new JsonSerializedKeysDictionaryJsonConverter<TDictionary, TKey, TValue>(factory),
                ConstructorType.FactoryWithCapacity => new JsonSerializedKeysDictionaryJsonConverter<TDictionary, TKey, TValue>(factoryWithCapacity),
                _ => throw new NotSupportedException()
            };
        }

        private static void AssertDictionary<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue>? actual)
        {
            actual.Should().NotBeNullOrEmpty();
            actual.Should().NotBeSameAs(expected);
            actual.Should().HaveCount(expected.Count);
            actual.Should().BeEquivalentTo(expected);
        }

        private static Dictionary<SampleKey, string> DictionaryFactory() => new Dictionary<SampleKey, string>();
        private static Dictionary<SampleKey, string> DictionaryFactoryWithCapacity(int capacity) => new Dictionary<SampleKey, string>(capacity);
        private static ConcurrentDictionary<SampleKey, string> ConcurrentDictionaryFactory() => new ConcurrentDictionary<SampleKey, string>();
        private static ConcurrentDictionary<SampleKey, string> ConcurrentDictionaryFactoryWithCapacity(int capacity) => new ConcurrentDictionary<SampleKey, string>(concurrencyLevel: Environment.ProcessorCount, capacity);
    }
}