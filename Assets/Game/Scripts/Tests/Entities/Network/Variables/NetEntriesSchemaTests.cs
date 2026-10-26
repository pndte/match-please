using System;
using Bw.Entities.Network.Variables;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetEntriesSchemaTests
    {
        [Test]
        public void SchemaHashIsTheFirstDeclaration()
        {
            var schema = Schema(builder => builder.DeclareRequest<int>(NetworkDelivery.Reliable));

            Assert.That(schema.Declarations[0], Is.SameAs(schema.HashDeclaration));
            Assert.That(schema.Declarations.Count, Is.EqualTo(2));
        }

        [Test]
        public void DeclarationKnowsTheFileAndLineItWasDeclaredAt()
        {
            var schema = Schema(builder => builder.DeclareRequest<int>(NetworkDelivery.Reliable));

            Assert.That(schema.Declarations[1].Site, Does.Match(@"^NetEntriesSchemaTests\.cs:\d+$"));
        }

        [Test]
        public void SameDeclarationsGiveTheSameHash()
        {
            static void Declare(INetEntriesSchemaBuilder builder)
            {
                builder.DeclareRequest<int>(NetworkDelivery.Unreliable);
                builder.DeclareResult<bool>(NetworkDelivery.Reliable);
                builder.DeclareProperty(0f, NetworkDelivery.ReliableSequenced, NetworkPermissions.Owner);
            }

            Assert.That(HashOf(Declare), Is.EqualTo(HashOf(Declare)));
        }

        [Test]
        public void InitialValuesDoNotChangeTheHash()
        {
            var zero = HashOf(builder => Property(builder, 0, NetworkPermissions.Server));
            var seven = HashOf(builder => Property(builder, 7, NetworkPermissions.Server));

            Assert.That(seven, Is.EqualTo(zero));
        }

        [Test]
        public void DeliveryChangesTheHash()
        {
            var reliable = HashOf(builder => Result<int>(builder, NetworkDelivery.Reliable));
            var unreliable = HashOf(builder => Result<int>(builder, NetworkDelivery.Unreliable));

            Assert.That(unreliable, Is.Not.EqualTo(reliable));
        }

        [Test]
        public void PermissionsChangeTheHash()
        {
            var server = HashOf(builder => Property(builder, 0, NetworkPermissions.Server));
            var owner = HashOf(builder => Property(builder, 0, NetworkPermissions.Owner));
            var everyone = HashOf(builder => Property(builder, 0, NetworkPermissions.Everyone));

            Assert.That(owner, Is.Not.EqualTo(server));
            Assert.That(everyone, Is.Not.EqualTo(server));
            Assert.That(everyone, Is.Not.EqualTo(owner));
        }

        [Test]
        public void ValueTypeChangesTheHash()
        {
            var ints = HashOf(builder => Result<int>(builder, NetworkDelivery.Reliable));
            var floats = HashOf(builder => Result<float>(builder, NetworkDelivery.Reliable));

            Assert.That(floats, Is.Not.EqualTo(ints));
        }

        [Test]
        public void ExtraEntryChangesTheHash()
        {
            var one = HashOf(builder => Result<int>(builder, NetworkDelivery.Reliable));
            var two = HashOf(builder =>
            {
                Result<int>(builder, NetworkDelivery.Reliable);
                Result<int>(builder, NetworkDelivery.Reliable);
            });

            Assert.That(two, Is.Not.EqualTo(one));
        }

        [Test]
        public void DeclarationSiteChangesTheHash()
        {
            var here = HashOf(builder => builder.DeclareResult<int>(NetworkDelivery.Reliable));
            var there = HashOf(builder => builder.DeclareResult<int>(NetworkDelivery.Reliable));

            Assert.That(there, Is.Not.EqualTo(here));
        }

        [Test]
        public void SwappedLookalikeDeclarationsChangeTheHash()
        {
            static void Ownership(INetEntriesSchemaBuilder builder) =>
                builder.DeclareResult<ulong>(NetworkDelivery.Reliable);

            static void Control(INetEntriesSchemaBuilder builder) =>
                builder.DeclareResult<ulong>(NetworkDelivery.Reliable);

            var ownershipFirst = HashOf(builder =>
            {
                Ownership(builder);
                Control(builder);
            });
            var controlFirst = HashOf(builder =>
            {
                Control(builder);
                Ownership(builder);
            });

            Assert.That(controlFirst, Is.Not.EqualTo(ownershipFirst));
        }

        private static void Property(INetEntriesSchemaBuilder builder, int initial, NetworkPermissions permissions) =>
            builder.DeclareProperty(initial, NetworkDelivery.Reliable, permissions);

        private static void Result<T>(INetEntriesSchemaBuilder builder, NetworkDelivery delivery) =>
            builder.DeclareResult<T>(delivery);

        private static uint HashOf(Action<INetEntriesSchemaBuilder> declare) =>
            Schema(declare).Hash;

        private static NetEntriesSchema Schema(Action<INetEntriesSchemaBuilder> declare)
        {
            var builder = new NetEntriesSchemaBuilder();
            declare(builder);
            return builder.Build();
        }
    }
}
