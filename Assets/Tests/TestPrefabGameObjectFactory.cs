using NUnit.Framework;
using System;
using UnityEngine;
using WM.Application;

namespace Tests
{
    public class TestPrefabGameObjectFactory
    {
        [Test]
        public void TestPrefabGameObjectFactoryRegisterAndCreate()
        {
            // Create factory.
            var factory = new PrefabGameObjectFactory();

            // Generate keys.
            var key1 = Guid.NewGuid();
            var key2 = Guid.NewGuid();

            {
                // Create prefabs.
                var prefab1 = new GameObject("One");
                var prefab2 = new GameObject("Two");

                // Register prefabs.
                factory.Register(key1, prefab1);
                factory.Register(key2, prefab2);
            }

            var product1 = factory.Create(key1, Vector3.zero, Quaternion.identity);
            Assert.AreEqual("One(Clone)", product1.name); 
            
            var product2 = factory.Create(key2, Vector3.zero, Quaternion.identity);
            Assert.AreEqual("Two(Clone)", product2.name);
        }

        [Test]
        public void TestPrefabGameObjectFactoryTryCreateNonRegistered()
        {
            // Create factory.
            var factory = new PrefabGameObjectFactory();

            var nonRegisteredKey = Guid.NewGuid();

            try
            {
                var product1 = factory.Create(nonRegisteredKey, Vector3.zero, Quaternion.identity);

                Assert.IsFalse(true); // We should not reach this!
            }
            catch(Exception e)
            {
                Assert.AreEqual(e.Message, "No prefab registered for key (" + nonRegisteredKey.ToString() + ").");
            }
        }

        [Test]
        public void TestPrefabGameObjectFactoryTryRegisterNull()
        {
            // Create factory.
            var factory = new PrefabGameObjectFactory();

            try
            {
                factory.Register(Guid.NewGuid(), null);

                Assert.IsFalse(true); // We should not reach this!
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "GameObject cannot be null.");
            }
        }

        [Test]
        public void TestPrefabGameObjectFactoryTryRegisterKeyEmpty()
        {
            // Create factory.
            var factory = new PrefabGameObjectFactory();

            try
            {
                factory.Register(Guid.Empty, new GameObject());

                Assert.IsFalse(true); // We should not reach this!
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "Key cannot be Guid.Empty.");
            }
        }
    }
}
