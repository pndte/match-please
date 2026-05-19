using System;

namespace Bw.Entities.Network.Variables
{
    [Flags]
    public enum NetworkPermissions : byte
    {
        Server = 1,
        Owner = 2,
        Client = 4 | Owner,
        Everyone = Server | Client
    }
}