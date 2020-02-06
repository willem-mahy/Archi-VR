using System;
using System.Collections.Generic;
using System.Net;

namespace WM.Net
{
    public class NetUtil
    {
        /// <summary>
        /// Short version of a GUID.
        /// To be used for debug logging purposes only - the short ID is NOT guaranteed to be unique!
        /// </summary>
        public static string ShortID(Guid guid)
        {
            return guid.ToString().Substring(0, 4);
        }

        /// <summary>
        /// Gets all IP adresses for the given host entry, as a list of strings.
        /// </summary>
        /// <param name="hostEntry"></param>
        /// <returns></returns>
        public static List<string> GetAllIPAdressesAsString(IPHostEntry hostEntry)
        {
            var r = new List<string>();

            foreach (var ipAddress in hostEntry.AddressList)
            {
                r.Add(ipAddress.ToString());
            }

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIPAddress()
        {
            // Get host name for local machine.
            var hostName = Dns.GetHostName();

            // Get host entry from host name.
            var hostEntry = Dns.GetHostEntry(hostName);

            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // Return first IP v4 address.
                {
                    return ip;
                }
            }

            throw new WebException("IP v4 address for local host not found!");
        }
            
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPSubNet()
        {
            var address = GetLocalIPAddress().ToString();

            return address.Substring(0, address.LastIndexOf('.') + 1);
        }
    }
}