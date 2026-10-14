using System;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Setup.Launch
{
    public static class NetworkManagerExtensions
    {
        public static void AdviseConnectionEvents(this NetworkManager manager, Lifetime lifetime, Action<ConnectionEventData> handler)
        {
            Action<NetworkManager, ConnectionEventData> handle = (_, connection) => handler(connection);
            lifetime.Bracket(() => manager.OnConnectionEvent += handle, () => manager.OnConnectionEvent -= handle);
        }

        public static void AdviseClientStopped(this NetworkManager manager, Lifetime lifetime, Action handler)
        {
            Action<bool> handle = _ => handler();
            lifetime.Bracket(() => manager.OnClientStopped += handle, () => manager.OnClientStopped -= handle);
        }
    }
}
