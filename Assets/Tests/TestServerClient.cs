using ArchiVR.Application;
using ArchiVR.Net;
using NUnit.Framework;
using System.Collections;
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
            // Create an application instance that will act as server.
            var applicationServerGO = new GameObject();
            var applicationServer = applicationServerGO.AddComponent(typeof(ApplicationArchiVR)) as ApplicationArchiVR;

            // Create an application instance that will connect as client.
            var applicationClientGO = new GameObject();
            var applicationClient = applicationClientGO.AddComponent(typeof(ApplicationArchiVR)) as ApplicationArchiVR;

            // Create the server for the server application.
            var serverGO = new GameObject();
            var server = applicationServerGO.AddComponent(typeof(ServerArchiVR)) as ServerArchiVR;
            server.application = applicationServer;

            Assert.AreEqual(0, server.NumClients);

            server.Init();

            Assert.AreEqual(0, server.NumClients);

            /*
            var client0GO = new GameObject();
            var client0 = client0GO.AddComponent(typeof(ClientArchiVR)) as ClientArchiVR;
            client0.application = applicationClient;

            Assert.AreEqual(0, server.NumClients);
            Assert.AreEqual(false, client0.Connected);

            
            // Let client A connect
            client0.Init();

            Thread.Sleep(1000);

            // Server should now have 1 client
            Assert.AreEqual(1, server.NumClients);

            // Client A should be connected.
            Assert.AreEqual(true, client0.Connected);

            client0.Disconnect();

            Thread.Sleep(1000);
            
            // Client A should be disconnected.
            Assert.AreEqual(false, client0.Connected);

            */

            // Server should have no clients connected.
            Assert.AreEqual(0, server.NumClients);

            server.Shutdown();
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
