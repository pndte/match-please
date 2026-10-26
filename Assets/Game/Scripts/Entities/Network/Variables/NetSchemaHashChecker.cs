using System;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetSchemaHashChecker
    {
        private readonly NetEntriesSchema _schema;
        private readonly IClientDisconnector _disconnector;

        public NetSchemaHashChecker(
            Lifetime lifetime,
            NetEntriesSchema schema,
            NetEntries entries,
            IClientDisconnector disconnector)
        {
            _schema = schema;
            _disconnector = disconnector;

            entries.Request(schema.HashDeclaration).ReceivedFromClient.Advise(lifetime, Check);
        }

        private void Check((ulong SenderClientId, uint Request) hash)
        {
            if (hash.Request == _schema.Hash)
                return;

            var reason = $"Net entries schema {hash.Request.ToString("X8")} of client {hash.SenderClientId.ToString()} differs from the server's {_schema.Hash.ToString("X8")}: " +
                         "the peers run different builds, which declare other entries or in another order.";
            _disconnector.Disconnect(hash.SenderClientId, reason);
            throw new InvalidOperationException(reason);
        }
    }
}
