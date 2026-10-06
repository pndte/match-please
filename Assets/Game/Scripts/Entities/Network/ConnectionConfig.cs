using System;

namespace Bw.Entities.Network
{
    [Serializable]
    public sealed class ConnectionConfig
    {
        public string ServerAddress = "127.0.0.1";
        public ushort Port = 7777;
    }
}
