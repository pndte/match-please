using System;
using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Mortality.Doubles
{
    public sealed class PropertyEntries<T>
    {
        public INetEntries Entries { get; }
        public NetPropertyDeclaration<T> Declaration { get; }
        public INetProperty<T> Property => Entries.Get(Declaration);

        public PropertyEntries(Lifetime lifetime, HiddenObjects objects, T initial)
        {
            var schema = new NetEntriesSchemaBuilder();
            Declaration = schema.DeclareProperty(initial, NetworkDelivery.ReliableSequenced, NetworkPermissions.Everyone);
            var networkObject = objects.Create("net entries").AddComponent<NetworkObject>();
            Entries = new NetVariablesTableClient(lifetime, networkObject, schema.Build(), new UnsentMessages(), new NotMine());
        }

        private sealed class UnsentMessages : IClientSendersCollection
        {
            public IClientMessageSender<TMessage> Get<TMessage>() =>
                throw new InvalidOperationException("An unspawned object sends nothing.");
        }

        private sealed class NotMine : IOwnership
        {
            public IReadonlyProperty<bool> Mine { get; } = new ViewableProperty<bool>(false);

            public void SetMine(Lifetime lifetime) =>
                throw new NotSupportedException("The test object is never owned here.");
        }
    }
}
