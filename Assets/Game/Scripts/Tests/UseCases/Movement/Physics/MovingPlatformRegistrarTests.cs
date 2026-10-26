using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement.Physics;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Physics
{
    public sealed class MovingPlatformRegistrarTests
    {
        private LifetimeDefinition _lifetime;
        private MovingPlatformCollection _platforms;
        private MovedPlatform _platform;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _platforms = new MovingPlatformCollection();
            _platform = new MovedPlatform(new Rect(0f, 0f, 1f, 1f), Vector2.zero);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void RegisteredPlatformIsListed()
        {
            new MovingPlatformRegistrar(_lifetime.Lifetime, _platform, _platforms);

            Assert.That(_platforms, Is.EqualTo(new[] { _platform }));
        }

        [Test]
        public void PlatformLeavesTheListWithItsLifetime()
        {
            var registered = _lifetime.Lifetime.CreateNested();
            new MovingPlatformRegistrar(registered.Lifetime, _platform, _platforms);

            registered.Terminate();

            Assert.That(_platforms, Is.Empty);
        }
    }
}
