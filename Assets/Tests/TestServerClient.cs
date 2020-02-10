using ArchiVR.Application;
using ArchiVR.Net;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WM.Command;
using WM.Net;

namespace Tests
{
    public class TestServerClient
    {
        // List of applications.
        List<ApplicationArchiVR> applications = new List<ApplicationArchiVR>();

        // Application instance that will act as server.
        ApplicationArchiVR applicationServer;

        // pplication instance that will connect as client 1.
        ApplicationArchiVR applicationClient1;

        // Application instance that will connect as client 2.
        ApplicationArchiVR applicationClient2;

        private static Guid DefaultAvatarID = Guid.NewGuid();
        private static Guid Avatar1ID = Guid.NewGuid();
        private static Guid Avatar2ID = Guid.NewGuid();

        #region Utility functions

        /// <summary>
        /// Create an ApplicationArchiVR with a Server and Client set.
        /// </summary>
        /// <returns></returns>
        private ApplicationArchiVR CreateApplication(string name)
        {
            ApplicationArchiVR.UnitTestModeEnabled = true;

            // Create an application instance that will act as server.
            var applicationGO = new GameObject();
            applicationGO.name = name;
            var application = applicationGO.AddComponent(typeof(ApplicationArchiVR)) as ApplicationArchiVR;

            application.Logger.Header("Create application '" + name + "'");

            application.DefaultAvatarID = DefaultAvatarID;
            

            application.AvatarFactory.Register(DefaultAvatarID, "DefaultAvatar");
            application.AvatarFactory.Register(Avatar2ID, "Avatar1");
            application.AvatarFactory.Register(Avatar1ID, "Avatar2");

            // Create the server for the server application.
            var serverGO = new GameObject();
            var server = applicationGO.AddComponent(typeof(ServerArchiVR)) as ServerArchiVR;
            server.applicationArchiVR = application;
            application.Server = server;

            var clientGO = new GameObject();
            var client = clientGO.AddComponent(typeof(ClientArchiVR)) as ClientArchiVR;
            client.applicationArchiVR = application;
            client.MessageProcessor = application;
            application.Client = client;

            application.m_centerEyeAnchor = new GameObject();
            application.m_leftHandAnchor = new GameObject();
            application.m_rightHandAnchor = new GameObject();
            
            application.StartupNetworkMode = NetworkMode.Standalone;

            application.Start();

            application.SetPlayerName(name + " player");

            Assert.AreEqual(0, application.Server.NumClients);

            Assert.IsFalse(application.Client.Connected);

            return application;
        }

        /// <summary>
        /// Call 'Update()' on all tested applications for a fixed number of times.
        /// After each call, sleep for a while.
        /// </summary>
        /// <param name="count"></param>
        private void UpdateApplications(int count = 50, int sleepMillis = 20)
        {
            for (int i = 0; i < count; ++i)
            {
                foreach (var application in applications) 
                    application.Update();

                Thread.Sleep(sleepMillis);
            }
        }

        private void StopServer()
        {
            applicationServer.Logger.Header("Stop Server");

            // Make server application initialize network mode from 'Server' to 'Standalone'.
            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);
            applicationServer.QueueCommand(new InitNetworkCommand(NetworkMode.Standalone));
            Assert.AreEqual(NetworkMode.Server, applicationServer.NetworkMode);

            UpdateApplications(); // Make queued commands execute.

            // server should be in 'standalone' network mode.
            Assert.AreEqual(NetworkMode.Standalone, applicationServer.NetworkMode);

            // server's local running client should be disconnected.
            Assert.IsFalse(applicationServer.Client.Connected);

            // Server should have no Client connected.
            Assert.AreEqual(0, applicationServer.Server.NumClients);
        }

        #endregion Utility functions

        // Test:
        // - Creating a Server and Client.
        // - Connecting the Client to the Server.
        // - Disconnecting the Client from the Server.
        // - Shutting down Client and Server.
        [Test]
        public void Test_ArchiVR_Multiplay_NominalWorkflow_Full_2Clients()
        {
            var log = new WM.Logger();

            #region Setup

            // Create an application instance that will act as server.
            applicationServer = CreateApplication("Server");
            applications.Add(applicationServer);

            // Create an application instance that will connect as client 1.
            applicationClient1 = CreateApplication("Client1");
            applications.Add(applicationClient1);

            // Create an application instance that will connect as client 2.
            applicationClient2 = CreateApplication("Client2");
            applications.Add(applicationClient2);

            #endregion

            // Disable logging.
            foreach (var application in applications)
            {
                application.Logger.Enabled = false;
            }

            #region Check initial application state

            Assert.AreEqual(0, applicationServer.Server.NumClients);
            Assert.AreEqual(0, applicationClient1.Server.NumClients); 
            Assert.AreEqual(0, applicationClient2.Server.NumClients);

            Assert.IsFalse(applicationServer.Client.Connected);
            Assert.IsFalse(applicationClient1.Client.Connected);
            Assert.IsFalse(applicationClient2.Client.Connected);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Player.AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Player.AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Player.AvatarID);

            Assert.AreEqual("Server player", applicationServer.Player.Name);
            Assert.AreEqual("Client1 player", applicationClient1.Player.Name);
            Assert.AreEqual("Client2 player", applicationClient2.Player.Name);

            #endregion Check initial application state

            // Enable logging.
            foreach (var application in applications)
            {
                application.Logger.Enabled = true;
            }

            #region Start Server.

            applications[0].Logger.Header("Start Server");

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

                // Server application is connected to the Server and introduced its own Player.
                Assert.AreEqual(1, applicationServer.Players.Count);
                Assert.AreEqual(0, applicationClient1.Players.Count);
                Assert.AreEqual(0, applicationClient2.Players.Count);
            }

            #endregion

            //WM.Logger.Enabled = true;

            #region Connect Client1

            applicationClient1.Logger.Header("Connect Client1");

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

                // Server and Client1 application are connected to the Server and introduced their own Player.
                Assert.AreEqual(2, applicationServer.Players.Count);
                Assert.AreEqual(2, applicationClient1.Players.Count);
                Assert.AreEqual(0, applicationClient2.Players.Count);
            }

            #endregion

            #region Connect Client2

            applicationClient2.Logger.Header("Connect Client2");

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

                // All clients are connected to the server and introduced their own Player.
                Assert.AreEqual(3, applicationServer.Players.Count);
                Assert.AreEqual(3, applicationClient1.Players.Count);
                Assert.AreEqual(3, applicationClient2.Players.Count);
            }

            #endregion

            #region Change server Avatar

            applicationServer.Logger.Header("Set Server avatar to Avatar1");
            applicationServer.SetPlayerAvatar(Avatar1ID);
            
            UpdateApplications(); // Make queued commands execute.

            // Server should now have changed avatar on all connected applications.
            Assert.AreEqual(Avatar1ID, applicationServer.Player.AvatarID);
            Assert.AreEqual(Avatar1ID, applicationClient1.Players[applicationServer.Player.ID].AvatarID);
            Assert.AreEqual(Avatar1ID, applicationClient2.Players[applicationServer.Player.ID].AvatarID);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Players[applicationClient1.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Player.AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Players[applicationClient1.Player.ID].AvatarID);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Players[applicationClient2.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Players[applicationClient2.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Player.AvatarID);

            #endregion

            #region Change server player name

            applicationServer.Logger.Header("Change Server player name");
            applicationServer.SetPlayerName("New Server player");

            UpdateApplications(); // Make queued commands execute.

            // Server should now have changed avatar on all connected applications.
            Assert.AreEqual(Avatar1ID, applicationServer.Player.AvatarID);
            Assert.AreEqual(Avatar1ID, applicationClient1.Players[applicationServer.Player.ID].AvatarID);
            Assert.AreEqual(Avatar1ID, applicationClient2.Players[applicationServer.Player.ID].AvatarID);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Players[applicationClient1.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Player.AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Players[applicationClient1.Player.ID].AvatarID);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Players[applicationClient2.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Players[applicationClient2.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Player.AvatarID);

            Assert.AreEqual("New Server player", applicationServer.Player.Name);
            Assert.AreEqual("New Server player", applicationClient1.Players[applicationServer.Player.ID].Name);
            Assert.AreEqual("New Server player", applicationClient2.Players[applicationServer.Player.ID].Name);

            Assert.AreEqual("Client1 player", applicationClient1.Player.Name);
            Assert.AreEqual("Client2 player", applicationClient2.Player.Name);

            #endregion

            #region Change Client1 player name

            applicationClient1.Logger.Header("Change Server player name");
            applicationClient1.SetPlayerName("New Client1 player");

            UpdateApplications(); // Make queued commands execute.

            // Server should now have changed avatar on all connected applications.
            Assert.AreEqual(Avatar1ID, applicationServer.Player.AvatarID);
            Assert.AreEqual(Avatar1ID, applicationClient1.Players[applicationServer.Player.ID].AvatarID);
            Assert.AreEqual(Avatar1ID, applicationClient2.Players[applicationServer.Player.ID].AvatarID);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Players[applicationClient1.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Player.AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Players[applicationClient1.Player.ID].AvatarID);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Players[applicationClient2.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Players[applicationClient2.Player.ID].AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Player.AvatarID);

            // Check Server player name
            Assert.AreEqual("New Server player", applicationServer.Player.Name);
            Assert.AreEqual("New Server player", applicationClient1.Players[applicationServer.Player.ID].Name);
            Assert.AreEqual("New Server player", applicationClient2.Players[applicationServer.Player.ID].Name);

            // Check Client1 player name
            Assert.AreEqual("New Client1 player", applicationServer.Players[applicationClient1.Player.ID].Name);
            Assert.AreEqual("New Client1 player", applicationClient1.Player.Name);
            Assert.AreEqual("New Client1 player", applicationClient2.Players[applicationClient1.Player.ID].Name);

            // Check Client2 player name
            Assert.AreEqual("Client2 player", applicationServer.Players[applicationClient2.Player.ID].Name);
            Assert.AreEqual("Client2 player", applicationClient1.Players[applicationClient2.Player.ID].Name);
            Assert.AreEqual("Client2 player", applicationClient2.Player.Name);

            #endregion

            //WM.Logger.Enabled = true;

            /*
            #region Test player state synchronisation

            var playerServerHeadPosition = new Vector3(1, 2, 3);
            var playerClient1HeadPosition = new Vector3(4, 5, 6);
            var playerClient2HeadPosition = new Vector3(7, 8, 9);

            // WHEN player state is updated
            applicationServer.m_centerEyeAnchor.transform.position = playerServerHeadPosition;
            applicationClient1.m_centerEyeAnchor.transform.position = playerClient1HeadPosition;
            applicationClient2.m_centerEyeAnchor.transform.position = playerClient2HeadPosition;

            UpdateApplications();

            WM.Logger.Enabled = false;

            foreach (var application in applications)
            {
                if (application != applicationServer)
                    Assert.AreEqual(playerServerHeadPosition, application.Players[applicationServer.Player.ID].Avatar.Head.transform.position);

                if (application != applicationClient1)
                    Assert.AreEqual(playerClient1HeadPosition, application.Players[applicationClient1.Player.ID].Avatar.Head.transform.position);

                if (application != applicationClient2) 
                    Assert.AreEqual(playerClient2HeadPosition, application.Players[applicationClient2.Player.ID].Avatar.Head.transform.position);
            }

            #endregion Test player state synchronisation
            */

            #region Disconnect Client2

            applicationClient2.Logger.Header("Disconnect Client2");

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

            // Server and Client1 application are connected to the Server, and introduced their own Player.
            Assert.AreEqual(2, applicationServer.Players.Count);
            Assert.AreEqual(2, applicationClient1.Players.Count);
            Assert.AreEqual(0, applicationClient2.Players.Count);

            #endregion

            #region Disconnect Client1

            applicationClient1.Logger.Header("Disconnect Client1");

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

            // Server application is connected to the Server, and introduced its own Player.
            Assert.AreEqual(1, applicationServer.Players.Count);
            Assert.AreEqual(0, applicationClient1.Players.Count);
            Assert.AreEqual(0, applicationClient2.Players.Count);

            #endregion

            StopServer();
        }

        // Test:
        // - Creating a Server and Client.
        // - Connecting the Client to the Server.
        // - Disconnecting the Client from the Server.
        // - Shutting down Client and Server.
        [Test]
        public void Test_ArchiVR_MultiPlay_ReinitAfterServerShutdown()
        {
            #region Setup

            // Create an application instance that will act as server.
            applicationServer = CreateApplication("Server");
            applications.Add(applicationServer);

            // Create an application instance that will connect as client 1.
            applicationClient1 = CreateApplication("Client1");
            applications.Add(applicationClient1);

            // Create an application instance that will connect as client 2.
            applicationClient2 = CreateApplication("Client2");
            applications.Add(applicationClient2);

            #endregion

            #region Check initial application state

            Assert.AreEqual(0, applicationServer.Server.NumClients);
            Assert.AreEqual(0, applicationClient1.Server.NumClients);
            Assert.AreEqual(0, applicationClient2.Server.NumClients);

            Assert.IsFalse(applicationServer.Client.Connected);
            Assert.IsFalse(applicationClient1.Client.Connected);
            Assert.IsFalse(applicationClient2.Client.Connected);

            Assert.AreEqual(DefaultAvatarID, applicationServer.Player.AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient1.Player.AvatarID);
            Assert.AreEqual(DefaultAvatarID, applicationClient2.Player.AvatarID);

            Assert.AreEqual("Server player", applicationServer.Player.Name);
            Assert.AreEqual("Client1 player", applicationClient1.Player.Name);
            Assert.AreEqual("Client2 player", applicationClient2.Player.Name);

            #endregion Check initial application state

            foreach (var application in applications)
            {
                application.Logger.Enabled = true;
            }

            #region Init network (Server + 2 Clients)

            #region Start Server.

            applicationServer.Logger.Header("Start Server");

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

                // Server application is connected to the Server and introduced its own Player.
                Assert.AreEqual(1, applicationServer.Players.Count);
                Assert.AreEqual(0, applicationClient1.Players.Count);
                Assert.AreEqual(0, applicationClient2.Players.Count);
            }

            #endregion

            #region Connect Client1

            applicationClient1.Logger.Header("Connect Client1");

            // WHEN the Client1 application is initialized to 'Client' network mode...
            {
                Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
                applicationClient1.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
                Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);

                UpdateApplications(); // Make queued commands execute.
            }

            // ... THEN ...
            {
                // ... its network mode is 'Client'
                Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);

                // ... its Server is not Running
                // TODO: Assert.AreEqual(ServerState.Running, applicationClient1.Server.State);

                // ... its Client is connected to the Server application's Server.
                Assert.IsTrue(applicationClient1.Client.Connected);
                Assert.AreEqual(2, applicationServer.Server.NumClients);

                // Server and Client1 application are connected to the Server and introduced their own Player.
                Assert.AreEqual(2, applicationServer.Players.Count);
                Assert.AreEqual(2, applicationClient1.Players.Count);
                Assert.AreEqual(0, applicationClient2.Players.Count);
            }

            #endregion

            #region Connect Client2

            applicationClient2.Logger.Header("Connect Client2");

            // WHEN the Client2 application is initialized to 'Client' network mode...
            {
                Assert.AreEqual(NetworkMode.Standalone, applicationClient2.NetworkMode);
                applicationClient2.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
                Assert.AreEqual(NetworkMode.Standalone, applicationClient2.NetworkMode);

                UpdateApplications(); // Make queued commands execute.
            }

            // ... THEN ...
            {
                // ... its network mode is 'Client'
                Assert.AreEqual(NetworkMode.Client, applicationClient2.NetworkMode);

                // ... its Server is not Running
                // TODO: Assert.AreEqual(ServerState.Running, applicationClient2.Server.State);

                // ... its Client is connected to the Server application's Server.
                Assert.IsTrue(applicationClient2.Client.Connected);
                Assert.AreEqual(3, applicationServer.Server.NumClients);

                // All clients are connected to the server and introduced their own Player.
                Assert.AreEqual(3, applicationServer.Players.Count);
                Assert.AreEqual(3, applicationClient1.Players.Count);
                Assert.AreEqual(3, applicationClient2.Players.Count);
            }

            #endregion

            #endregion Init network (Server + 2 Clients)

            // Shutdown Network (Without disconnecting Clients first)
            StopServer();

            #region Re-init network (Server + 2 Clients)

            #region Restart Server.

            applicationServer.Logger.Header("Restart Server");

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

                // Server application is connected to the Server and introduced its own Player.
                Assert.AreEqual(1, applicationServer.Players.Count);
                Assert.AreEqual(0, applicationClient1.Players.Count);
                Assert.AreEqual(0, applicationClient2.Players.Count);
            }

            #endregion Restart Server.

            #region Connect Client1

            applicationClient1.Logger.Header("Connect Client1");

            // The server will run at different ports after restart, so clear the stale ServerInfo cached in the Client.
            applicationClient1.Client.ServerInfo = null;

            // WHEN the Client1 application is initialized to 'Client' network mode...
            {
                Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);
                applicationClient1.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
                Assert.AreEqual(NetworkMode.Standalone, applicationClient1.NetworkMode);

                UpdateApplications(); // Make queued commands execute.
            }

            // ... THEN ...
            {
                // ... its network mode is 'Client'
                Assert.AreEqual(NetworkMode.Client, applicationClient1.NetworkMode);

                // ... its Server is not Running
                // TODO: Assert.AreEqual(ServerState.Running, applicationClient1.Server.State);

                // ... its Client is connected to the Server application's Server.
                Assert.IsTrue(applicationClient1.Client.Connected);
                Assert.AreEqual(2, applicationServer.Server.NumClients);

                // Server and Client1 application are connected to the Server and introduced their own Player.
                Assert.AreEqual(2, applicationServer.Players.Count);
                Assert.AreEqual(2, applicationClient1.Players.Count);
                Assert.AreEqual(0, applicationClient2.Players.Count);
            }

            #endregion

            #region Connect Client2

            applicationClient2.Logger.Header("Connect Client2");

            // The server will run at different ports after restart, so clear the stale ServerInfo cached in the Client.
            applicationClient2.Client.ServerInfo = null;

            // WHEN the Client2 application is initialized to 'Client' network mode...
            {
                Assert.AreEqual(NetworkMode.Standalone, applicationClient2.NetworkMode);
                applicationClient2.QueueCommand(new InitNetworkCommand(NetworkMode.Client));
                Assert.AreEqual(NetworkMode.Standalone, applicationClient2.NetworkMode);

                UpdateApplications(); // Make queued commands execute.
            }

            // ... THEN ...
            {
                // ... its network mode is 'Client'
                Assert.AreEqual(NetworkMode.Client, applicationClient2.NetworkMode);

                // ... its Server is not Running
                // TODO: Assert.AreEqual(ServerState.Running, applicationClient2.Server.State);

                // ... its Client is connected to the Server application's Server.
                Assert.IsTrue(applicationClient2.Client.Connected);
                Assert.AreEqual(3, applicationServer.Server.NumClients);

                // All clients are connected to the server and introduced their own Player.
                Assert.AreEqual(3, applicationServer.Players.Count);
                Assert.AreEqual(3, applicationClient1.Players.Count);
                Assert.AreEqual(3, applicationClient2.Players.Count);
            }

            #endregion Connect Client2

            #endregion Re-init network (Server + 2 Clients)

            #region Shutdown Network (Disconnecting Clients first)

            #region Disconnect Client2

            applicationClient2.Logger.Header("Disconnect Client2");

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

            // Server and Client1 application are connected to the Server, and introduced their own Player.
            Assert.AreEqual(2, applicationServer.Players.Count);
            Assert.AreEqual(2, applicationClient1.Players.Count);
            Assert.AreEqual(0, applicationClient2.Players.Count);

            #endregion Disconnect Client2

            #region Disconnect Client1

            applicationClient1.Logger.Header("Disconnect Client1");

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

            // Server application is connected to the Server, and introduced its own Player.
            Assert.AreEqual(1, applicationServer.Players.Count);
            Assert.AreEqual(0, applicationClient1.Players.Count);
            Assert.AreEqual(0, applicationClient2.Players.Count);

            #endregion Disconnect Client1

            #region Stop Server

            applicationServer.Logger.Header("Stop Server");

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

            #endregion Stop Server

            #endregion Shutdown Network (Disconnecting Clients first)
        }
    }
}
