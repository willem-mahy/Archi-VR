using ArchiVR.Application;
using ArchiVR.Net;
using NUnit.Framework;
using System.Threading;
using UnityEngine;
using WM.Command;
using WM.Net;

namespace Tests
{
    public class TestServerClient
    {
        /// <summary>
        /// Create an ApplicationArchiVR with a Server and Client set.
        /// </summary>
        /// <returns></returns>
        private ApplicationArchiVR CreateApplication()
        {
            // Create an application instance that will act as server.
            var applicationGO = new GameObject();
            var application = applicationGO.AddComponent(typeof(ApplicationArchiVR)) as ApplicationArchiVR;

            // Create the server for the server application.
            var serverGO = new GameObject();
            var server = applicationGO.AddComponent(typeof(ServerArchiVR)) as ServerArchiVR;
            server.application = application;
            application.Server = server;

            var clientGO = new GameObject();
            var client = clientGO.AddComponent(typeof(ClientArchiVR)) as ClientArchiVR;
            client.application = application;
            application.Client = client;

            application.StartupNetworkMode = NetworkMode.Standalone;

            application.Start();

            Assert.AreEqual(0, application.Server.NumClients);
            
            Assert.IsFalse(application.Client.Connected);

            return application;
        }

        // Test:
        // - Creating a Server and Client.
        // - Connecting the Client to the Server.
        // - Disconnecting the Client from the Server.
        // - Shutting down Client and Server.
        [Test]
        public void TestServerClientConnect()
        {
            #region Setup

            // Create an application instance that will act as server.
            var applicationServer = CreateApplication();

            // Create an application instance that will connect as client 1.
            var applicationClient1 = CreateApplication();

            // Create an application instance that will connect as client 2.
            var applicationClient2 = CreateApplication();

            #endregion

            Assert.AreEqual(0, applicationServer.Server.NumClients);
            Assert.AreEqual(0, applicationClient1.Server.NumClients); 
            Assert.AreEqual(0, applicationClient2.Server.NumClients);

            Assert.IsFalse(applicationServer.Client.Connected);
            Assert.IsFalse(applicationClient1.Client.Connected);
            Assert.IsFalse(applicationClient2.Client.Connected);

            // Make server application initialize network mode from 'Standalone' to 'Server'.
            Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);
            applicationServer.QueueCommand(new InitNetworkCommand(NetworkMode.Server));
            Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);
            applicationServer.Update();
            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);

            // Make client1 application initialize network mode from 'Standalone' to 'Client'.
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
            applicationClient1.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);

            for (int i = 0; i < 15; ++i)
            {
                Thread.Sleep(100);
                applicationClient1.Update();
                applicationClient2.Update();
                applicationServer.Update();
            }
            

            Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);
            Assert.IsTrue(applicationClient1.Client.Connected);

            Assert.AreEqual(1, applicationServer.Server.NumClients);

            // Make client2 application initialize network mode from 'Standalone' to 'Client'.
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
            applicationClient2.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);

            for (int i = 0; i < 15; ++i)
            {
                Thread.Sleep(100);
                applicationClient1.Update();
                applicationClient2.Update();
                applicationServer.Update();
            }

            Assert.AreEqual(NetworkMode.Client, applicationClient2.NetworkMode);
            Assert.IsTrue(applicationClient2.Client.Connected);

            Assert.AreEqual(2, applicationServer.Server.NumClients);

            /*
            
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

            // Server should have no clients connected.
            Assert.AreEqual(0, server.NumClients);

            server.Shutdown();
            */
        }

        //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        //// `yield return null;` to skip a frame.
        //[UnityTest]
        //public IEnumerator TestServerClientWithEnumeratorPasses()
        //{
        //    // Use the Assert class to test conditions.
        //    // Use yield to skip a frame.
        //    yield return null;
        //}
    }
}
