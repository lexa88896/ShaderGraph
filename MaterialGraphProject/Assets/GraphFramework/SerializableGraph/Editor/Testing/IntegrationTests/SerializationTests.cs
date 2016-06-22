using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Graphing;

namespace UnityEditor.Graphing.IntegrationTests
{
    [TestFixture]
    public class SerializationTests
    {
        interface ITestInterface
        {}

        [Serializable]
        class SimpleSerializeClass : ITestInterface
        {
            [SerializeField]
            public string stringValue;
            [SerializeField]
            public int intValue;
            [SerializeField]
            public float floatValue;
            [SerializeField]
            public int[] arrayValue;

            public static SimpleSerializeClass instance
            {
                get
                {
                    return new SimpleSerializeClass
                    {
                        stringValue = "ABCD",
                        intValue = 5,
                        floatValue = 7.7f,
                        arrayValue = new[] {1, 2, 3, 4}
                    };
                }
            }

            public virtual void AssertAsReference()
            {
                var reference = instance;
                Assert.AreEqual(reference.stringValue, stringValue);
                Assert.AreEqual(reference.intValue, intValue);
                Assert.AreEqual(reference.floatValue, floatValue);
                Assert.AreEqual(reference.arrayValue.Length, arrayValue.Length);
                Assert.AreEqual(reference.arrayValue, arrayValue);
            }
        }

        [Serializable]
        class ChildClassA : SimpleSerializeClass
        {
            [SerializeField]
            public string childString;

            public new static ChildClassA instance
            {
                get
                {
                    return new ChildClassA
                    {
                        stringValue = "qwee",
                        intValue = 5,
                        floatValue = 6f,
                        arrayValue = new[] {5, 6, 7, 8},
                        childString = "CHILD"
                    };
                }
            }

            public override void AssertAsReference()
            {
                var reference = instance;
                Assert.AreEqual(reference.stringValue, stringValue);
                Assert.AreEqual(reference.intValue, intValue);
                Assert.AreEqual(reference.floatValue, floatValue);
                Assert.AreEqual(reference.arrayValue.Length, arrayValue.Length);
                Assert.AreEqual(reference.arrayValue, arrayValue);
                Assert.AreEqual(reference.childString, childString);
            }
        }

        [Serializable]
        class ChildClassB : SimpleSerializeClass
        {
            [SerializeField]
            public int childInt;

            public new static ChildClassB instance
            {
                get
                {
                    return new ChildClassB
                    {
                        stringValue = "qwee",
                        intValue = 5,
                        floatValue = 6f,
                        arrayValue = new[] {5, 6, 7, 8},
                        childInt = 666
                    };
                }
            }

            public override void AssertAsReference()
            {
                var reference = instance;
                Assert.AreEqual(reference.stringValue, stringValue);
                Assert.AreEqual(reference.intValue, intValue);
                Assert.AreEqual(reference.floatValue, floatValue);
                Assert.AreEqual(reference.arrayValue.Length, arrayValue.Length);
                Assert.AreEqual(reference.arrayValue, arrayValue);
                Assert.AreEqual(reference.childInt, childInt);
            }
        }

        [Serializable]
        class SerializationContainer
        {
            public List<SerializationHelper.JSONSerializedElement> serializedElements;
        }

        [Test]
        public void TestSerializationHelperCanSerializeThenDeserialize()
        {
            var toSerialize = new List<SimpleSerializeClass>()
            {
                SimpleSerializeClass.instance
            };

            var serialized = SerializationHelper.Serialize(toSerialize);
            Assert.AreEqual(1, serialized.Count);

            var loaded = SerializationHelper.Deserialize<SimpleSerializeClass>(serialized);
            Assert.AreEqual(1, loaded.Count);
            Assert.IsInstanceOf<SimpleSerializeClass>(loaded[0]);
            loaded[0].AssertAsReference();
        }

        [Test]
        public void TestPolymorphicSerializationPreservesTypesViaBaseClass()
        {
            var toSerialize = new List<SimpleSerializeClass>()
            {
                SimpleSerializeClass.instance,
                ChildClassA.instance,
                ChildClassB.instance
            };

            var serialized = SerializationHelper.Serialize(toSerialize);
            Assert.AreEqual(3, serialized.Count);

            var loaded = SerializationHelper.Deserialize<SimpleSerializeClass>(serialized);
            Assert.AreEqual(3, loaded.Count);
            Assert.IsInstanceOf<SimpleSerializeClass>(loaded[0]);
            Assert.IsInstanceOf<ChildClassA>(loaded[1]);
            Assert.IsInstanceOf<ChildClassB>(loaded[2]);
            loaded[0].AssertAsReference();
            loaded[1].AssertAsReference();
            loaded[2].AssertAsReference();
        }

        [Test]
        public void TestPolymorphicSerializationPreservesTypesViaInterface()
        {
            var toSerialize = new List<ITestInterface>()
            {
                SimpleSerializeClass.instance,
                ChildClassA.instance,
                ChildClassB.instance
            };

            var serialized = SerializationHelper.Serialize(toSerialize);
            Assert.AreEqual(3, serialized.Count);

            var loaded = SerializationHelper.Deserialize<SimpleSerializeClass>(serialized);
            Assert.AreEqual(3, loaded.Count);
            Assert.IsInstanceOf<SimpleSerializeClass>(loaded[0]);
            Assert.IsInstanceOf<ChildClassA>(loaded[1]);
            Assert.IsInstanceOf<ChildClassB>(loaded[2]);
            loaded[0].AssertAsReference();
            loaded[1].AssertAsReference();
            loaded[2].AssertAsReference();
        }

        [Test]
        public void TestSerializationHelperElementCanSerialize()
        {
            var toSerialize = new List<SimpleSerializeClass>()
            {
                SimpleSerializeClass.instance
            };
            
            var serialized = SerializationHelper.Serialize(toSerialize);
            Assert.AreEqual(1, serialized.Count);

            var container = new SerializationContainer
            {
                serializedElements = serialized
            };

            var serializedContainer = JsonUtility.ToJson(container, true);

            var deserializedContainer = JsonUtility.FromJson<SerializationContainer>(serializedContainer);
            var loaded = SerializationHelper.Deserialize<SimpleSerializeClass>(deserializedContainer.serializedElements);
            Assert.AreEqual(1, loaded.Count);
            Assert.IsInstanceOf<SimpleSerializeClass>(loaded[0]);
            loaded[0].AssertAsReference();
        }
        
    }
}