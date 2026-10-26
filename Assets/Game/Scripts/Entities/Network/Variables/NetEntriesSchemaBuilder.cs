using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public interface INetEntriesSchemaBuilder
    {
        public NetRequestDeclaration<T> DeclareRequest<T>(
            NetworkDelivery deliveryType,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0);

        public NetResultDeclaration<T> DeclareResult<T>(
            NetworkDelivery deliveryType,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0);

        public NetPropertyDeclaration<T> DeclareProperty<T>(
            T initial,
            NetworkDelivery deliveryType,
            NetworkPermissions permissions,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0);
    }

    public sealed class NetEntriesSchemaBuilder : INetEntriesSchemaBuilder
    {
        private const string HashSite = "schema hash";

        private readonly NetRequestDeclaration<uint> _hash = new(NetworkDelivery.Reliable, NetworkPermissions.Everyone, HashSite);
        private readonly List<NetEntryDeclaration> _declarations;
        private bool _built;

        public NetEntriesSchemaBuilder()
        {
            _declarations = new List<NetEntryDeclaration> { _hash };
        }

        public NetRequestDeclaration<T> DeclareRequest<T>(
            NetworkDelivery deliveryType,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var declaration = new NetRequestDeclaration<T>(deliveryType, NetworkPermissions.Client, SiteOf(file, line));
            Add(declaration);
            return declaration;
        }

        public NetResultDeclaration<T> DeclareResult<T>(
            NetworkDelivery deliveryType,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var declaration = new NetResultDeclaration<T>(deliveryType, SiteOf(file, line));
            Add(declaration);
            return declaration;
        }

        public NetPropertyDeclaration<T> DeclareProperty<T>(
            T initial,
            NetworkDelivery deliveryType,
            NetworkPermissions permissions,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var declaration = new NetPropertyDeclaration<T>(initial, deliveryType, permissions, SiteOf(file, line));
            Add(declaration);
            return declaration;
        }

        public NetEntriesSchema Build()
        {
            ThrowIfBuilt();
            _built = true;
            return new NetEntriesSchema(_hash, _declarations.ToArray());
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

        private static string SiteOf(string file, int line) =>
            $"{file.Substring(Math.Max(file.LastIndexOf('/'), file.LastIndexOf('\\')) + 1)}:{line.ToString()}";
    }
}
