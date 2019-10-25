using System.Net;

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
            
            public static string GetLocalIPSubNet()
            {
                var address = GetLocalIPAddress();

                return address.Substring(0, address.LastIndexOf('.') + 1);
            }
        }
    } // namespace Net
} // namespace WM