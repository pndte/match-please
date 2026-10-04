using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public interface INetEntriesSchemaBuilder
    {
        public NetSignalDeclaration<T> DeclareSignal<T>(NetworkDelivery deliveryType, NetworkPermissions permissions);
        public NetPropertyDeclaration<T> DeclareProperty<T>(T initial, NetworkDelivery deliveryType, NetworkPermissions permissions);
    }

    public sealed class NetEntriesSchemaBuilder : INetEntriesSchemaBuilder
    {
        private readonly List<NetEntryDeclaration> _declarations = new();
        private bool _built;

        public NetSignalDeclaration<T> DeclareSignal<T>(NetworkDelivery deliveryType, NetworkPermissions permissions)
        {
            var declaration = new NetSignalDeclaration<T>(deliveryType, permissions);
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
