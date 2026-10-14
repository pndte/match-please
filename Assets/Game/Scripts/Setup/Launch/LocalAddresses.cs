using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Setup.Launch
{
    public static class LocalAddresses
    {
        private const string RadminPrefix = "26.";

        public static bool IsRadmin(string address) =>
            address.StartsWith(RadminPrefix);

        public static string[] Read()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    .Where(address => address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                    .Select(address => address.ToString())
                    .OrderByDescending(address => address.StartsWith(RadminPrefix))
                    .ToArray();
            }
            catch (SocketException)
            {
                return Array.Empty<string>();
            }
        }
    }
}
