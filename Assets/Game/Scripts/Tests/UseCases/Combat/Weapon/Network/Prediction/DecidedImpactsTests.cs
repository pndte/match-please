using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Rollback;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Network.Prediction;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Prediction
{
    public sealed class DecidedImpactsTests
    {
        private LifetimeDefinition _lifetime;
        private FakeStrikes _strikes;
        private ScriptedWorldStep _world;
        private PredictedBodies _bodies;
        private FakeCharacter _actor;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _strikes = new FakeStrikes();
            _world = new ScriptedWorldStep();
            _bodies = new PredictedBodies();
            _actor = new FakeCharacter();
            new DecidedImpacts(_lifetime.Lifetime, _strikes, _world, _bodies, new WorldRollbackConfig { MaxReplayTicks = 8 });
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void StrikeKnocksEveryPredictedBodyItAffectsAtOnce()
        {
            var first = new FakeMortal();
            var second = new FakeMortal();
            var firstBody = _bodies.Add(first);
            var secondBody = _bodies.Add(second);

            _strikes.Strike(FakeStrikes.Of(10, _actor, first, second));

            Assert.That(firstBody.Damages, Is.EqualTo(new[] { 1f }));
            Assert.That(secondBody.Damages, Is.EqualTo(new[] { 2f }));
        }

        [Test]
        public void KnockHasNoActionCause()
        {
            var target = new FakeMortal();
            var body = _bodies.Add(target);

            _strikes.Strike(FakeStrikes.Of(10, _actor, target));

            Assert.That(body.Uncaused, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void TargetWithoutAPredictedBodyIsLeftAlone()
        {
            var predicted = new FakeMortal();
            var body = _bodies.Add(predicted);

            _strikes.Strike(FakeStrikes.Of(10, _actor, new FakeMortal(), predicted));

            Assert.That(body.Damages, Is.EqualTo(new[] { 2f }));
        }

        [Test]
        public void ReplayRepeatsTheDecisionOfTheReplayedTick()
        {
            var target = new FakeMortal();
            var body = _bodies.Add(target);
            _strikes.Strike(FakeStrikes.Of(10, _actor, target));

            _world.Replay(9, 10, 11);

            Assert.That(body.Damages, Is.EqualTo(new[] { 1f, 1f }));
        }

        [Test]
        public void LiveTickNeverRepeatsADecision()
        {
            var target = new FakeMortal();
            var body = _bodies.Add(target);
            _strikes.Strike(FakeStrikes.Of(10, _actor, target));

            _world.Step(10);
            _world.Step(11);

            Assert.That(body.Damages, Is.EqualTo(new[] { 1f }));
        }

        [Test]
        public void ReplayWithoutADecisionKnocksNothing()
        {
            var target = new FakeMortal();
            var body = _bodies.Add(target);

            _world.Replay(9, 10);

            Assert.That(body.Damages, Is.Empty);
        }

        [Test]
        public void TickIsDecidedOnce()
        {
            var target = new FakeMortal();
            var body = _bodies.Add(target);
            _strikes.Strike(FakeStrikes.Of(10, _actor, target));
            LogAssert.Expect(LogType.Exception, new Regex("decided once a tick"));

            _strikes.Strike(FakeStrikes.Of(10, _actor, target));

            Assert.That(body.Damages, Is.EqualTo(new[] { 1f }));
        }

        [Test]
        public void StrikesOfDifferentTicksAreDecidedApart()
        {
            var target = new FakeMortal();
            var body = _bodies.Add(target);
            _strikes.Strike(FakeStrikes.Of(10, _actor, target));
            _strikes.Strike(FakeStrikes.Of(11, _actor, target));

            _world.Replay(10, 11);

            Assert.That(body.Damages, Is.EqualTo(new[] { 1f, 1f, 1f, 1f }));
        }

        private sealed class PredictedBodies : IAffectables<IReadonlyMortal, Impacts>
        {
            private readonly Dictionary<IReadonlyMortal, RecordingBody> _bodies = new();

            public RecordingBody Add(IReadonlyMortal target)
            {
                var body = new RecordingBody();
                _bodies.Add(target, body);
                return body;
            }

            public void AddLifetimed(
                Lifetime lifetime,
                IReadonlyMortal target,
                ulong networkObjectId,
                IAffectable<Impacts> part) =>
                throw new NotSupportedException("The test registers its bodies itself.");

            public bool TryGet(IReadonlyMortal target, out ulong networkObjectId, out IAffectable<Impacts> affectable)
            {
                networkObjectId = 0;
                var found = _bodies.TryGetValue(target, out var body);
                affectable = body;
                return found;
            }
        }

        private sealed class RecordingBody : IAffectable<Impacts>
        {
            public List<float> Damages { get; } = new();
            public List<bool> Uncaused { get; } = new();

            public void Affect(StateCause cause, Impacts effect)
            {
                Damages.Add(effect[0].Hit.Damage);
                cause.Switch(Uncaused, uncaused => uncaused.Add(true), (uncaused, _) => uncaused.Add(false));
            }
        }
    }
}
