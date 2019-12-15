using ArchiVR.Net;
using NUnit.Framework;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TestServerClient
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestServerClientConnect()
        {
            var serverGO = new GameObject();
            var server = serverGO.AddComponent(typeof(ServerArchiVR)) as ServerArchiVR;

            Assert.AreEqual(0, server.NumClients);

            server.Init();

            Assert.AreEqual(0, server.NumClients);

            var client0GO = new GameObject();
            var client0 = client0GO.AddComponent(typeof(ClientArchiVR)) as ClientArchiVR;
            
            Assert.AreEqual(0, server.NumClients);
            Assert.AreEqual(false, client0.Connected);

            client0.Init();

            Thread.Sleep(100);

            Assert.AreEqual(1, server.NumClients);
            Assert.AreEqual(true, client0.Connected);

        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestServerClientWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
