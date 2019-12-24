using ArchiVR.Application;
using ArchiVR.Net;
using NUnit.Framework;
using System;
using System.Threading;
using UnityEngine;
using WM.Command;
using WM.Net;

namespace Tests
{
    public class TestServerClient
    {
        public int ClientBasePort = 8800;

        private static Guid DefaultAvatarID = Guid.NewGuid();

        private static GameObject DefaultAvatarPrefab = CreateMockAvatarPrefab("DefaultAvatar");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static GameObject CreateMockAvatarPrefab(String name)
        {
            GameObject avatarGO = new GameObject(name);

            GameObject avatarHead = new GameObject();
            avatarHead.transform.SetParent(avatarGO.transform);

            GameObject avatarHandL = new GameObject();
            avatarHandL.transform.SetParent(avatarGO.transform);

            GameObject avatarHandR = new GameObject();
            avatarHandR.transform.SetParent(avatarGO.transform);

            var avatar = avatarGO.AddComponent(typeof(WM.Net.Avatar)) as WM.Net.Avatar;

            return avatarGO;
        }
        
        /// <summary>
        /// Create an ApplicationArchiVR with a Server and Client set.
        /// </summary>
        /// <returns></returns>
        private ApplicationArchiVR CreateApplication(int clientBasePort)
        {
            // Create an application instance that will act as server.
            var applicationGO = new GameObject();
            var application = applicationGO.AddComponent(typeof(ApplicationArchiVR)) as ApplicationArchiVR;

            application.DefaultAvatarID = DefaultAvatarID;

            application.AvatarFactory.Register(DefaultAvatarID, DefaultAvatarPrefab);

            // Create the server for the server application.
            var serverGO = new GameObject();
            var server = applicationGO.AddComponent(typeof(ServerArchiVR)) as ServerArchiVR;
            server.application = application;
            application.Server = server;

            var clientGO = new GameObject();
            var client = clientGO.AddComponent(typeof(ClientArchiVR)) as ClientArchiVR;
            client.BasePort = clientBasePort;
            client.application = application;
            application.Client = client;

            application.StartupNetworkMode = NetworkMode.Standalone;

            application.Start();

            Assert.AreEqual(0, application.Server.NumClients);
            
            Assert.IsFalse(application.Client.Connected);

            return application;
        }

        // Application instance that will act as server.
        ApplicationArchiVR applicationServer;

        // pplication instance that will connect as client 1.
        ApplicationArchiVR applicationClient1;

        // Application instance that will connect as client 2.
        ApplicationArchiVR applicationClient2;

        /// <summary>
        /// Call 'Update()' on all tested applications for a fixed number of times.
        /// After each call, sleep for a while.
        /// </summary>
        /// <param name="count"></param>
        private void UpdateApplications(int count = 20, int sleepMillis = 20)
        {
            for (int i = 0; i < count; ++i)
            {
                applicationClient1.Update();
                applicationClient2.Update();
                applicationServer.Update();
                Thread.Sleep(sleepMillis);
            }
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
            applicationServer = CreateApplication(ClientBasePort);

            // Create an application instance that will connect as client 1.
            applicationClient1 = CreateApplication(ClientBasePort + 10);

            // Create an application instance that will connect as client 2.
            applicationClient2 = CreateApplication(ClientBasePort + 20);

            #endregion

            Assert.AreEqual(0, applicationServer.Server.NumClients);
            Assert.AreEqual(0, applicationClient1.Server.NumClients); 
            Assert.AreEqual(0, applicationClient2.Server.NumClients);

            Assert.IsFalse(applicationServer.Client.Connected);
            Assert.IsFalse(applicationClient1.Client.Connected);
            Assert.IsFalse(applicationClient2.Client.Connected);

            #region Start Server.

            // Make server application initialize network mode from 'Standalone' to 'Server'.
            Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);
            applicationServer.QueueCommand(new InitNetworkCommand(NetworkMode.Server));
            Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);
            Assert.IsTrue(applicationServer.Client.Connected);

            // Starting the Server automatically connects the local-running Client to it.
            Assert.AreEqual(1, applicationServer.Server.NumClients);

            #endregion

            // Make client1 application initialize network mode from 'Standalone' to 'Client'.
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
            applicationClient1.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);
            Assert.IsTrue(applicationClient1.Client.Connected);

            Assert.AreEqual(2, applicationServer.Server.NumClients);

            /*
            // Make client2 application initialize network mode from 'Standalone' to 'Client'.
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
            applicationClient2.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);

            // Make the command to transition networkmode, execute.
            UpdateApplications(10);

            Assert.AreEqual(NetworkMode.Client, applicationClient2.NetworkMode);
            Assert.IsTrue(applicationClient2.Client.Connected);

            Assert.AreEqual(2, applicationServer.Server.NumClients);
            */

            #region Disconnect Remote Client 1

            //applicationClient1.Client.Disconnect();

            // Make client1 application initialize network mode from 'Client' to 'Standalone'.
            Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);
            applicationClient1.QueueCommand(new InitNetworkCommand(NetworkMode.Standalone));
            Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            //Thread.Sleep(1000);

            // Remote Client 1 should be disconnected.
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
            Assert.AreEqual(false, applicationClient1.Client.Connected);

            // Server should have 1 client connected (it's own).            
            Assert.AreEqual(1, applicationServer.Server.NumClients);

            #endregion

            #region Disconnect Server Client

            //applicationServer.Client.Disconnect(); // Taken care of by InitNetworkCommand

            // Make client1 application initialize network mode from 'Server' to 'Standalone'.
            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);
            applicationServer.QueueCommand(new InitNetworkCommand(NetworkMode.Standalone));
            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            //Thread.Sleep(1000);

            // Server should be in 'Standalone' network mode.
            Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);

            // Server's local running Client should be disconnected.
            Assert.IsFalse(applicationServer.Client.Connected);

            // Server should have no Client connected.
            Assert.AreEqual(0, applicationServer.Server.NumClients);

            #endregion

            #region Shutdown Server.

            applicationServer.Server.Shutdown();
            // FIXME: TODO: Assert.AreEqual(ServerState.Down, applicationServer.Server.State);

            #endregion
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
