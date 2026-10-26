using System;
using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public static class UnspawnedNetEntries
    {
        public static INetEntries Create(GameObject owner, NetEntriesSchema schema) =>
            new NetEntries(schema, new NetSendGuardFactoryClient(new NobodysOwnership(), owner.AddComponent<NetworkObject>()));

        private sealed class NobodysOwnership : IOwnership
        {
            public IReadonlyProperty<bool> Mine { get; } = new ViewableProperty<bool>(false);

            public void SetMine(Lifetime lifetime) =>
                throw new NotSupportedException("An unspawned table is owned by nobody.");
        }
    }
}
