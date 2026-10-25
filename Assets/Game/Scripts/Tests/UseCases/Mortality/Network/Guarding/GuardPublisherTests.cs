using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardPublisherTests
    {
        private static readonly GuardState Initial = new(100f, false, false);

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private PropertyEntries<GuardState> _entries;
        private SpawnedObject _object;
        private ShownGuard _guard;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _entries = new PropertyEntries<GuardState>(_lifetime.Lifetime, _objects, Initial);
            _object = new SpawnedObject(_entries.Entries);
            _guard = new ShownGuard();
            _ = new GuardPublisher(_lifetime.Lifetime, _object, _guard, _entries.Entries, _entries.Declaration);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void UnspawnedGuardPublishesNothing()
        {
            _guard.Show(new GuardState(50f, true, false));

            Assert.That(_entries.Property.Value, Is.EqualTo(Initial));
        }

        [Test]
        public void SpawnedGuardPublishesItsStateAtOnce()
        {
            _guard.Show(new GuardState(50f, true, false));

            _object.Spawn(_lifetime.Lifetime);

            Assert.That(_entries.Property.Value, Is.EqualTo(new GuardState(50f, true, false)));
        }

        [Test]
        public void SpawnedGuardPublishesEveryChange()
        {
            _object.Spawn(_lifetime.Lifetime);

            _guard.Show(new GuardState(0f, true, true));

            Assert.That(_entries.Property.Value, Is.EqualTo(new GuardState(0f, true, true)));
        }

        [Test]
        public void DespawnedGuardStopsPublishing()
        {
            var spawned = _lifetime.Lifetime.CreateNested();
            _object.Spawn(spawned.Lifetime);
            spawned.Terminate();

            _guard.Show(new GuardState(20f, false, false));

            Assert.That(_entries.Property.Value, Is.EqualTo(Initial));
        }

        private sealed class ShownGuard : IReadonlyGuard
        {
            public float Max => 100f;
            public IReadonlyProperty<GuardState> State => _state;

            private readonly ViewableProperty<GuardState> _state = new(Initial);

            public void Show(GuardState state) =>
                _state.Value = state;
        }
    }
}
