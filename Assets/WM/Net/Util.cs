using System.Net;

namespace WM.Net
{
    public class NetUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        public static void PrintAllIPAdresses(IPHostEntry hostEntry)
        {
            WM.Logger.Debug("Host IP addresses:");

            foreach (var ipAddress in hostEntry.AddressList)
            {
                WM.Logger.Debug("    - " + ipAddress);
            }
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

            PrintAllIPAdresses(hostEntry);

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