using NUnit.Framework;
using System;
using System.Net.Sockets;
using UnityEngine;
using WM.Application;
using WM.Net;

namespace Tests
{
    public class TestMessage
    {
        /// <summary>
        /// Not really a unit test for Serialization/deserialization of Message objects.
        /// This test compares the performance of different serialization approaches for network transport using the Message class:
        /// - Approach 1:
        ///     - Step 1: Binary-serialize object into Message object's data member
        ///     - Step 2: Xml-serialize Message
        ///     - Send string with xml-serialized message to network
        /// - Approach 2:
        ///     - Binary-serialize object
        ///     - Append "<Message></Message>" tags arond string with binary-serialized object
        ///     - Send this string with to network
        ///     
        /// The tests show that the execution time of both approaches compares roughly as:
        ///     24(approach 1) versus 16(approach 2)
        /// </summary>
        [Test]
        public void MessageSerializeDeserialize()
        {
            var log = new WM.Logger();

            var udpClient = new UdpClient();

            var udpSend = new WM.Net.UDPSend(udpClient, log);
            udpSend.remoteIP = "127.0.0.1";
            udpSend.remotePort = 8890;
            udpSend.Init();

            var avatarState = new WM.Net.AvatarState();

            {
                // To skip one-time Xml serializer initialization.
                var messageXML = WM.Net.Message.EncodeObjectAsXml(avatarState);
            }

            var numIterations = 1000;

            {
                var start = DateTime.Now;

                for (int i = 0; i < numIterations; ++i)
                {
                    var messageXML = WM.Net.Message.EncodeObjectAsXml(avatarState);

                    udpSend.SendString(messageXML);
                }

                var end = DateTime.Now;

                log.Enabled = true;

                log.Debug("Sending " + numIterations + " times an XML Message encoded AvatarState took " + (end - start).TotalMilliseconds);
                
                log.Enabled = false;
            }

            {
                var MessageBeginTag = @"<Message>";
                var MessageEndTag = @"<\Message>";
                
                var start = DateTime.Now;

                for (int i = 0; i < numIterations; ++i)
                {
                    var message = Message.EncodeObject(avatarState);

                    udpSend.SendString(MessageBeginTag);

                    udpSend.Send(message.Data);

                    udpSend.SendString(MessageEndTag);
                }

                var end = DateTime.Now;

                log.Enabled = true;

                log.Debug("Sending " + numIterations + " times a Binary encoded AvatarState took " + (end - start).TotalMilliseconds);

                log.Enabled = false;
            }
        }
    }
}
