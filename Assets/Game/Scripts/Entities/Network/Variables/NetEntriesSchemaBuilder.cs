using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public interface INetEntriesSchemaBuilder
    {
        public NetRequestDeclaration<T> DeclareRequest<T>(NetworkDelivery deliveryType);
        public NetResultDeclaration<T> DeclareResult<T>(NetworkDelivery deliveryType);
        public NetPropertyDeclaration<T> DeclareProperty<T>(T initial, NetworkDelivery deliveryType, NetworkPermissions permissions);
    }

    public sealed class NetEntriesSchemaBuilder : INetEntriesSchemaBuilder
    {
        private readonly List<NetEntryDeclaration> _declarations = new();
        private bool _built;

        public NetRequestDeclaration<T> DeclareRequest<T>(NetworkDelivery deliveryType)
        {
            var declaration = new NetRequestDeclaration<T>(deliveryType);
            Add(declaration);
            return declaration;
        }

        public NetResultDeclaration<T> DeclareResult<T>(NetworkDelivery deliveryType)
        {
            var declaration = new NetResultDeclaration<T>(deliveryType);
            Add(declaration);
            return declaration;
        }

        public NetPropertyDeclaration<T> DeclareProperty<T>(T initial, NetworkDelivery deliveryType, NetworkPermissions permissions)
        {
            var declaration = new NetPropertyDeclaration<T>(initial, deliveryType, permissions);
            Add(declaration);
            return declaration;
        }

        public NetEntriesSchema Build()
        {
            ThrowIfBuilt();
            _built = true;
            return new NetEntriesSchema(_declarations.ToArray());
        }

        private void Add(NetEntryDeclaration declaration)
        {
            ThrowIfBuilt();
            _declarations.Add(declaration);
        }

        private void ThrowIfBuilt()
        {
            if (_built)
                throw new InvalidOperationException("The net entries schema is already built, every entry must be declared before Build.");
        }
    }
}
