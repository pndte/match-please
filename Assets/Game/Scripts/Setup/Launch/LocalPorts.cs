using System.Net;
using System.Net.Sockets;

namespace Setup.Launch
{
    public static class LocalPorts
    {
        public static bool IsFree(ushort port)
        {
            try
            {
                using var probe = new UdpClient(new IPEndPoint(IPAddress.Any, port));
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
