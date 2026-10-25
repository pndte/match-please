using System;
using Bw.Entities.Network.Variables;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetEntriesSchemaBuilderTests
    {
        [Test]
        public void EveryDeclarationIsItsOwnKey()
        {
            var builder = new NetEntriesSchemaBuilder();

            var first = builder.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Server);
            var second = builder.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Server);

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void SecondBuildThrows()
        {
            var builder = new NetEntriesSchemaBuilder();
            builder.Build();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Test]
        public void RequestDeclaredAfterBuildThrows()
        {
            var builder = new NetEntriesSchemaBuilder();
            builder.Build();

            Assert.Throws<InvalidOperationException>(() => builder.DeclareRequest<int>(NetworkDelivery.Reliable));
        }

        [Test]
        public void ResultDeclaredAfterBuildThrows()
        {
            var builder = new NetEntriesSchemaBuilder();
            builder.Build();

            Assert.Throws<InvalidOperationException>(() => builder.DeclareResult<int>(NetworkDelivery.Reliable));
        }

        [Test]
        public void PropertyDeclaredAfterBuildThrows()
        {
            var builder = new NetEntriesSchemaBuilder();
            builder.Build();

            Assert.Throws<InvalidOperationException>(() => builder.DeclareProperty(1, NetworkDelivery.Reliable, NetworkPermissions.Everyone));
        }
    }
}
