using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class PredictedAffectableRegistrarTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private Toggle _predicted;
        private Target _target;
        private RecordingAffectable<float> _part;
        private Affectables<Target, float> _affectables;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _predicted = new Toggle();
            _target = new Target();
            _part = new RecordingAffectable<float>();
            _affectables = new Affectables<Target, float>();
            new PredictedAffectableRegistrar<Target, float>(
                _lifetime.Lifetime,
                _predicted,
                _objects.Create("character").AddComponent<NetworkObject>(),
                _target,
                _part,
                _affectables);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void TargetNotPredictedHereIsNotAffectable()
        {
            Assert.That(_affectables.TryGet(_target, out _, out _), Is.False);
        }

        [Test]
        public void PredictedTargetIsAffectableUnderItsNetworkObject()
        {
            _predicted.Set(true);

            Assert.That(_affectables.TryGet(_target, out var id, out var affectable), Is.True);
            Assert.That(id, Is.EqualTo(0UL));
            affectable.Affect(StateCause.None, 3f);
            Assert.That(_part.Affected, Has.Count.EqualTo(1));
        }

        [Test]
        public void TargetLeavingThePredictionIsNoLongerAffectable()
        {
            _predicted.Set(true);

            _predicted.Set(false);

            Assert.That(_affectables.TryGet(_target, out _, out _), Is.False);
        }

        [Test]
        public void TargetComingBackIntoThePredictionIsAffectableAgain()
        {
            _predicted.Set(true);
            _predicted.Set(false);

            _predicted.Set(true);

            Assert.That(_affectables.TryGet(_target, out _, out _), Is.True);
        }

        [Test]
        public void EndedRegistrationRemovesTheTarget()
        {
            _predicted.Set(true);

            _lifetime.Terminate();

            Assert.That(_affectables.TryGet(_target, out _, out _), Is.False);
        }

        private sealed class Target
        {
        }
    }
}
