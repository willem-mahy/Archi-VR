using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

namespace WM
{
    namespace Net
    {
        class NetUtil
        {
            public static string GetLocalIPAddress()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                throw new WebException("Local IP address not found!");
            }
        }
    } // namespace Net
} // namespace WM