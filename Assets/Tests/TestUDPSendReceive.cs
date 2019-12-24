using NUnit.Framework;
using System.Net.Sockets;
using System.Threading;
using WM.Net;

namespace Tests
{
    public class TestUDPSendReceive
    {
        // Test:
        // - Creating a UDPReceiver and 2 UDPSender instances targeting the receiver.
        // - Sending messages from senders to the receiver.
        [Test]
        public void TestUDPSendReceive_ConnectAndSend()
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

            // Send 'Bye' message from sender 2
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
    }
}
