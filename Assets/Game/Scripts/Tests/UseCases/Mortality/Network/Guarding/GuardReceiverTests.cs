using System.Collections.Generic;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardReceiverTests
    {
        private static readonly GuardState Initial = new(100f, false, false);

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private PropertyEntries<GuardState> _entries;
        private AppliedGuard _guard;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _entries = new PropertyEntries<GuardState>(_lifetime.Lifetime, _objects, Initial);
            _guard = new AppliedGuard();
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void ReceiverAppliesTheKnownStateAtOnce()
        {
            _ = new GuardReceiver(_lifetime.Lifetime, _entries.Entries, _entries.Declaration, _guard);

            Assert.That(_guard.Applied, Is.EqualTo(new[] { Initial }));
        }

        [Test]
        public void ReceiverAppliesEveryReceivedState()
        {
            _ = new GuardReceiver(_lifetime.Lifetime, _entries.Entries, _entries.Declaration, _guard);

            _entries.Property.Value = new GuardState(40f, true, false);
            _entries.Property.Value = new GuardState(0f, true, true);

            Assert.That(_guard.Applied, Is.EqualTo(new[] { Initial, new GuardState(40f, true, false), new GuardState(0f, true, true) }));
        }

        [Test]
        public void ReceiverStopsWithItsLifetime()
        {
            var receiving = _lifetime.Lifetime.CreateNested();
            _ = new GuardReceiver(receiving.Lifetime, _entries.Entries, _entries.Declaration, _guard);
            receiving.Terminate();

            _entries.Property.Value = new GuardState(40f, true, false);

            Assert.That(_guard.Applied, Is.EqualTo(new[] { Initial }));
        }

        private sealed class AppliedGuard : IGuard
        {
            public float Max => 100f;
            public IReadonlyProperty<GuardState> State => _state;
            public List<GuardState> Applied { get; } = new();

            private readonly ViewableProperty<GuardState> _state = new(Initial);

            public void Apply(GuardState state)
            {
                Applied.Add(state);
                _state.Value = state;
            }
        }
    }
}
