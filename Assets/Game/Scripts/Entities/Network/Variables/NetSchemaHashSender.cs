using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetSchemaHashSender
    {
        public NetSchemaHashSender(
            Lifetime lifetime,
            NetEntriesSchema schema,
            INetworkLifetimedObject networkObject,
            INetEntries entries)
        {
            var hash = entries.RequestSender(schema.HashDeclaration);
            networkObject.SpawnedLifetime.WhenAlive(lifetime, _ => hash.Send(schema.Hash));
        }
    }
}
