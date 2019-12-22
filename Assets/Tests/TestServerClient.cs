using ArchiVR.Application;
using ArchiVR.Net;
using NUnit.Framework;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using WM.Net;

namespace Tests
{
    public class TestServerClient
    {
        // Test:
        // - Creating a UDPReceiver and 2 UDPSender instances targeting the receiver.
        // - Sending messages from senders to the receiver.
        [Test]
        public void TestUDPSendReceive()
        {
            var ip = "127.0.0.1";
            var receiverPort = 8880;
            var sender1Port = 8881;
            var sender2Port = 8882;
            
            var sender1Key = UDPReceive.GetRemoteEndpointKey(ip, sender1Port);
            var sender2Key = UDPReceive.GetRemoteEndpointKey(ip, sender2Port);

            var hello1 = "Hello from " + sender1Port;
            var hello2 = "Hello from " + sender2Port;
            var bye1 = "Bye from " + sender1Port;
            var bye2 = "Bye from " + sender2Port;

            #region Setup

            var receiverUdpClient = new UdpClient(receiverPort);
            var receiver = new UDPReceive(receiverUdpClient);
            receiver.Init();

            var sender1UdpClient = new UdpClient(sender1Port);
            var sender1 = new UDPSend(sender1UdpClient);
            sender1.remoteIP = ip;
            sender1.remotePort = receiverPort; 
            sender1.Init();

            var sender2UdpClient = new UdpClient(sender2Port);
            var sender2 = new UDPSend(sender2UdpClient);
            sender2.remoteIP = ip;
            sender2.remotePort = receiverPort;
            sender2.Init();

            #endregion

            // Check startup state.
            Assert.AreEqual(0, receiver.allReceivedUDPPackets.Count);
            Assert.AreEqual("", receiver.lastReceivedUDPPacket);

            // Send 'Hello' message from sender 1
            sender1.SendString(hello1);

            Thread.Sleep(100);

            Assert.AreEqual(1, receiver.allReceivedUDPPackets.Count);

            Assert.AreEqual(hello1, receiver.allReceivedUDPPackets[sender1Key]);

            Assert.AreEqual(hello1, receiver.lastReceivedUDPPacket);

            // Send 'Hello' message from sender 2
            sender2.SendString(hello2);

            Thread.Sleep(100);
            
            Assert.AreEqual(2, receiver.allReceivedUDPPackets.Count);

            Assert.AreEqual(hello2, receiver.allReceivedUDPPackets[sender2Key]);

            Assert.AreEqual(hello2, receiver.lastReceivedUDPPacket);
            
            // Send 'Bye' message from sender 1
            sender1.SendString(bye1);

            Thread.Sleep(100);

            Assert.AreEqual(hello1 + bye1, receiver.allReceivedUDPPackets[sender1Key]);

            Assert.AreEqual(bye1, receiver.lastReceivedUDPPacket);

            // Send 'Bye' message from sender 1
            sender2.SendString(bye2);

            Thread.Sleep(500);

            Assert.AreEqual(hello2 + bye2, receiver.allReceivedUDPPackets[sender2Key]);

            Assert.AreEqual(bye2, receiver.lastReceivedUDPPacket);
            
            #region Teardown

            receiver.Shutdown();
            
            receiverUdpClient.Close();
            sender1UdpClient.Close();
            sender2UdpClient.Close();

            #endregion
        }

        // Test:
        // - Creating a Server and Client.
        // - Connecting the Client to the Server.
        // - Disconnecting the Client from the Server.
        // - Shutting down Client and Server.
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

            // Server should have no clients connected.
            Assert.AreEqual(0, server.NumClients);

            server.Shutdown();
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
