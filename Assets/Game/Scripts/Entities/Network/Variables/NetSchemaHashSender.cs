using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Routing;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetSchemaHashSender
    {
        public NetSchemaHashSender(
            Lifetime lifetime,
            NetEntriesSchema schema,
            NetworkObject networkObject,
            INetworkLifetimedObject networkLifetimed,
            IClientNetworkRouter router)
        {
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, _ =>
                router.SendToServer(
                    new NetworkMessage<NetSchemaHash>(
                        new NetworkMessageHeader
                        {
                            NetworkObjectId = networkObject.NetworkObjectId,
                            VarId = NetVariablesTableBase.SchemaHashVarId
                        },
                        new NetSchemaHash { Value = schema.Hash }),
                    NetworkDelivery.Reliable));
        }
    }
}
