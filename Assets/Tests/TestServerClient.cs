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
        private static Guid DefaultAvatarID = Guid.NewGuid();
        private static Guid Avatar1ID = Guid.NewGuid();
        private static Guid Avatar2ID = Guid.NewGuid();

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
        private ApplicationArchiVR CreateApplication(string name)
        {
            // Create an application instance that will act as server.
            var applicationGO = new GameObject();
            applicationGO.name = name;
            var application = applicationGO.AddComponent(typeof(ApplicationArchiVR)) as ApplicationArchiVR;

            application.DefaultAvatarID = DefaultAvatarID;

            application.AvatarFactory.Register(DefaultAvatarID, CreateMockAvatarPrefab("DefaultAvatar"));
            application.AvatarFactory.Register(Avatar2ID, CreateMockAvatarPrefab("Avatar1"));
            application.AvatarFactory.Register(Avatar1ID, CreateMockAvatarPrefab("Avatar2"));

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
        private void UpdateApplications(int count = 50, int sleepMillis = 20)
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
            LogHeader("Initialize Server application");
            applicationServer = CreateApplication("Server");

            // Create an application instance that will connect as client 1.
            LogHeader("Initialize Remote Client 1 application");
            applicationClient1 = CreateApplication("Client1");

            // Create an application instance that will connect as client 2.
            LogHeader("Initialize Remote Client 2 application");
            applicationClient2 = CreateApplication("Client2");

            #endregion

            #region Check initial application state

            Assert.AreEqual(0, applicationServer.Server.NumClients);
            Assert.AreEqual(0, applicationClient1.Server.NumClients); 
            Assert.AreEqual(0, applicationClient2.Server.NumClients);

            Assert.IsFalse(applicationServer.Client.Connected);
            Assert.IsFalse(applicationClient1.Client.Connected);
            Assert.IsFalse(applicationClient2.Client.Connected);

            #endregion Check initial application state

            #region Start Server.
            
            LogHeader("Start Server");

            // WHEN the ServerApplication is initialized to 'Server' network mode...
            {
                Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);
                applicationServer.QueueCommand(new InitNetworkCommand(NetworkMode.Server));
                Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);

                UpdateApplications(); // Make queued commands execute.
            }

            // ... THEN ...
            {
                // ... its network mode is 'Server'
                Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);

                // ... its Server is Running
                // TODO: Assert.AreEqual(ServerState.Running, applicationServer.Server.State);

                // ... its Client is automatically connected to that server.
                Assert.AreEqual(1, applicationServer.Server.NumClients);
                Assert.IsTrue(applicationServer.Client.Connected);

                // None of the application is connected to the Server, so none of them has remote users.
                Assert.AreEqual(0, applicationServer.remoteUsers.Count);
                Assert.AreEqual(0, applicationClient1.remoteUsers.Count);
                Assert.AreEqual(0, applicationClient2.remoteUsers.Count);
            }

            #endregion

            #region Connect Client1'.
            
            LogHeader("Connect Client1");

            // WHEN the Client1 application is initialized to 'Client' network mode...
            {
                Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
                applicationClient1.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
                Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);

                UpdateApplications(); // Make queued commands execute.
            }

            // ... THEN ...
            {
                // ... its network mode is 'Server'
                Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);

                // ... its Server is not Running
                // TODO: Assert.AreEqual(ServerState.Running, applicationClient1.Server.State);

                // ... its Client is connected to the Server application's Server.
                Assert.IsTrue(applicationClient1.Client.Connected);
                Assert.AreEqual(2, applicationServer.Server.NumClients);

                // Only Clients of Server and Client1 application are connected to the Server.
                Assert.AreEqual(1, applicationServer.remoteUsers.Count);
                Assert.AreEqual(1, applicationClient1.remoteUsers.Count);
                Assert.AreEqual(0, applicationClient2.remoteUsers.Count);
            }

            #endregion

            #region Connect Client2

            LogHeader("Connect Client2");

            // WHEN the Client2 application is initialized to 'Client' network mode...
            {
                Assert.AreEqual(NetworkMode.Standalone, applicationClient2.NetworkMode);
                applicationClient2.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
                Assert.AreEqual(NetworkMode.Standalone, applicationClient2.NetworkMode);

                UpdateApplications(); // Make queued commands execute.
            }

            // ... THEN ...
            {
                // ... its network mode is 'Server'
                Assert.AreEqual(NetworkMode.Client, applicationClient2.NetworkMode);

                // ... its Server is not Running
                // TODO: Assert.AreEqual(ServerState.Running, applicationClient2.Server.State);

                // ... its Client is connected to the Server application's Server.
                Assert.IsTrue(applicationClient2.Client.Connected);
                Assert.AreEqual(3, applicationServer.Server.NumClients);

                // All clients (Client of Server, Client1 and CLient2 application) are connected to the server.
                // They should all list the other 2 as remote users.
                Assert.AreEqual(2, applicationServer.remoteUsers.Count);
                Assert.AreEqual(2, applicationClient1.remoteUsers.Count);
                Assert.AreEqual(2, applicationClient2.remoteUsers.Count);
            }

            #endregion

            #region Change server Avatar

            //LogHeader("Change server Avatar");

            //applicationServer.SetAvatar(Avatar1ID);

            #endregion

            #region Disconnect Client2

            LogHeader("Disconnect Client2");

            // Make client1 application initialize network mode from 'Client' to 'Standalone'.
            Assert.AreEqual(NetworkMode.Client, applicationClient2.NetworkMode);
            applicationClient2.QueueCommand(new InitNetworkCommand(NetworkMode.Standalone));
            Assert.AreEqual(NetworkMode.Client, applicationClient2.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            // Remote Client 1 should be disconnected.
            Assert.AreEqual(NetworkMode.Standalone, applicationClient2.NetworkMode);
            Assert.AreEqual(false, applicationClient2.Client.Connected);

            // Server should have 1 client connected (it's own).            
            Assert.AreEqual(2, applicationServer.Server.NumClients);

            // Only Clients of Server and Client1 application are connected to the Server.
            Assert.AreEqual(1, applicationServer.remoteUsers.Count);
            Assert.AreEqual(1, applicationClient1.remoteUsers.Count);
            Assert.AreEqual(0, applicationClient2.remoteUsers.Count);

            #endregion

            #region Disconnect Client1

            LogHeader("Disconnect Client1");

            // Make client1 application initialize network mode from 'Client' to 'Standalone'.
            Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);
            applicationClient1.QueueCommand(new InitNetworkCommand(NetworkMode.Standalone));
            Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            // Remote Client 1 should be disconnected.
            Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
            Assert.AreEqual(false, applicationClient1.Client.Connected);

            // Server should have 1 client connected (it's own).            
            Assert.AreEqual(1, applicationServer.Server.NumClients);

            #endregion
            
            #region Stop Server

            LogHeader("Stop Server");

            // Make server application initialize network mode from 'Server' to 'Standalone'.
            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);
            applicationServer.QueueCommand(new InitNetworkCommand(NetworkMode.Standalone));
            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            // Server should be in 'Standalone' network mode.
            Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);

            // Server's local running Client should be disconnected.
            Assert.IsFalse(applicationServer.Client.Connected);

            // Server should have no Client connected.
            Assert.AreEqual(0, applicationServer.Server.NumClients);

            #endregion
        }

        private void LogHeader(string caption)
        {
            WM.Logger.Debug("");
            WM.Logger.Debug("=======[" + caption + "]===============================");
        }
    }
}
