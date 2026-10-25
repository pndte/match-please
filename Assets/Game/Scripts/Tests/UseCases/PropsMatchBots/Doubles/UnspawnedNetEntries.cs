using System;
using System.Collections.Generic;
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
        public static INetEntries Create(Lifetime lifetime, GameObject owner, NetEntriesSchema schema) =>
            new NetVariablesTableClient(
                lifetime,
                owner.AddComponent<NetworkObject>(),
                schema,
                new SendersCollection(new Dictionary<Type, IMessageSender>()),
                new NobodysOwnership());

        private sealed class NobodysOwnership : IOwnership
        {
            public IReadonlyProperty<bool> Mine { get; } = new ViewableProperty<bool>(false);

            public void SetMine(Lifetime lifetime) =>
                throw new NotSupportedException("An unspawned table is owned by nobody.");
        }
    }
}
