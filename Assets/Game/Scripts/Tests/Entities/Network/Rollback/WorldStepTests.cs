using System.Collections.Generic;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Rollback
{
    public sealed class WorldStepTests
    {
        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private List<string> _order;
        private WorldStep _step;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock();
            _order = new List<string>();
            _step = new WorldStep(_lifetime.Lifetime, _clock);
            new PhysicsStepper(_lifetime.Lifetime, new CountingSteps(() => _order.Add("physics")), _step);
            foreach (var phase in new[] { TickPhase.Prepare, TickPhase.Early, TickPhase.Default, TickPhase.Late, TickPhase.Publish })
                _step.Stepped(phase).Advise(_lifetime.Lifetime, tick => _order.Add($"{phase} {tick}"));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void TickFiresEveryPhaseInOrderWithPhysicsBetweenEarlyAndDefault()
        {
            _clock.Tick(5);

            Assert.That(_order, Is.EqualTo(new[] { "Prepare 5", "Early 5", "physics", "Default 5", "Late 5", "Publish 5" }));
        }

        [Test]
        public void PhysicsPhaseFiresWithTheTickBetweenEarlyAndDefault()
        {
            _step.Stepped(TickPhase.Physics).Advise(_lifetime.Lifetime, tick => _order.Add($"Physics {tick}"));

            _clock.Tick(5);

            Assert.That(_order.IndexOf("Physics 5"), Is.GreaterThan(_order.IndexOf("Early 5")));
            Assert.That(_order.IndexOf("Physics 5"), Is.LessThan(_order.IndexOf("Default 5")));
        }

        [Test]
        public void EveryTickIsSteppedOnce()
        {
            _clock.Advance(3);

            Assert.That(_order.FindAll(entry => entry.StartsWith("Default")), Is.EqualTo(new[] { "Default 1", "Default 2", "Default 3" }));
            Assert.That(_order.FindAll(entry => entry == "physics"), Has.Count.EqualTo(3));
        }

        [Test]
        public void ServerNeverReplays()
        {
            var replaying = new List<bool>();
            _step.Replaying.Advise(_lifetime.Lifetime, replaying.Add);

            _clock.Advance(3);

            Assert.That(replaying, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void TerminatedStepStopsFollowingTheClock()
        {
            _lifetime.Terminate();

            _clock.Advance();

            Assert.That(_order, Is.Empty);
        }
    }
}
