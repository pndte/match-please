using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Rollback
{
    public sealed class RollbackStepperTests
    {
        private const int MaxReplayTicks = 5;

        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private List<string> _order;
        private RollbackStepper _stepper;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock();
            _order = new List<string>();
            var config = new WorldRollbackConfig { MaxReplayTicks = MaxReplayTicks };
            _stepper = new RollbackStepper(_lifetime.Lifetime, _clock, config);
            new PhysicsStepper(_lifetime.Lifetime, new CountingSteps(() => _order.Add("physics")), _stepper);
            foreach (var phase in new[] { TickPhase.Prepare, TickPhase.Early, TickPhase.Default, TickPhase.Late, TickPhase.Publish })
                _stepper.Stepped(phase).Advise(_lifetime.Lifetime, tick => _order.Add($"{phase} {tick}"));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void TickWithoutDivergenceStepsOnceInOrder()
        {
            _clock.Tick(10);

            Assert.That(_order, Is.EqualTo(new[] { "Prepare 10", "Early 10", "physics", "Default 10", "Late 10", "Publish 10" }));
        }

        [Test]
        public void EveryTrackedPartIsSavedAtThePreviousTickBeforeTheStep()
        {
            var first = new ScriptedPart(_order, "first");
            var second = new ScriptedPart(_order, "second");
            _stepper.Track(_lifetime.Lifetime, first);
            _stepper.Track(_lifetime.Lifetime, second);

            _clock.Tick(10);

            Assert.That(_order, Is.EqualTo(new[] { "Prepare 10", "first saved 9", "second saved 9", "Early 10", "physics", "Default 10", "Late 10", "Publish 10" }));
        }

        [Test]
        public void DivergenceRestoresEveryPartAndReplaysUpToThePreviousTick()
        {
            var diverged = new ScriptedPart(_order, "diverged") { Divergence = 7 };
            var other = new ScriptedPart(_order, "other");
            _stepper.Track(_lifetime.Lifetime, diverged);
            _stepper.Track(_lifetime.Lifetime, other);

            _clock.Tick(10);

            Assert.That(_order, Is.EqualTo(new[]
            {
                "Prepare 10",
                "diverged saved 9", "other saved 9",
                "diverged restored 7", "other restored 7",
                "Early 8", "physics", "Default 8", "Late 8", "diverged followed 8", "other followed 8", "diverged saved 8", "other saved 8",
                "Early 9", "physics", "Default 9", "Late 9", "diverged followed 9", "other followed 9", "diverged saved 9", "other saved 9",
                "Early 10", "physics", "Default 10", "Late 10",
                "Publish 10"
            }));
        }

        [Test]
        public void ReplayRaisesReplayingOnlyWhileItLasts()
        {
            _stepper.Track(_lifetime.Lifetime, new ScriptedPart(_order, "part") { Divergence = 8 });
            var replaying = new List<(int tick, bool replaying)>();
            _stepper.Stepped(TickPhase.Default).Advise(_lifetime.Lifetime, tick => replaying.Add((tick, _stepper.Replaying.Value)));
            _stepper.Stepped(TickPhase.Publish).Advise(_lifetime.Lifetime, tick => replaying.Add((tick, _stepper.Replaying.Value)));

            _clock.Tick(10);

            Assert.That(replaying, Is.EqualTo(new[] { (9, true), (10, false), (10, false) }));
        }

        [Test]
        public void PrepareAndPublishAreNeverReplayed()
        {
            _stepper.Track(_lifetime.Lifetime, new ScriptedPart(_order, "part") { Divergence = 6 });

            _clock.Tick(10);

            Assert.That(_order.FindAll(entry => entry.StartsWith("Prepare")), Is.EqualTo(new[] { "Prepare 10" }));
            Assert.That(_order.FindAll(entry => entry.StartsWith("Publish")), Is.EqualTo(new[] { "Publish 10" }));
        }

        [Test]
        public void DivergenceOlderThanTheReplayWindowRestoresAtTheWindowEdge()
        {
            var part = new ScriptedPart(_order, "part") { Divergence = 10 - MaxReplayTicks - 3 };
            _stepper.Track(_lifetime.Lifetime, part);

            _clock.Tick(10);

            Assert.That(part.Restored, Is.EqualTo(new[] { 10 - MaxReplayTicks }));
            Assert.That(_order.FindAll(entry => entry.StartsWith("Default")), Has.Count.EqualTo(MaxReplayTicks));
        }

        [Test]
        public void DivergenceAtTheWindowEdgeIsReplayed()
        {
            var part = new ScriptedPart(_order, "part") { Divergence = 10 - MaxReplayTicks };
            _stepper.Track(_lifetime.Lifetime, part);

            _clock.Tick(10);

            Assert.That(part.Restored, Is.EqualTo(new[] { 10 - MaxReplayTicks }));
        }

        [Test]
        public void DivergenceAtTheCurrentTickIsNotReplayed()
        {
            var part = new ScriptedPart(_order, "part") { Divergence = 10 };
            _stepper.Track(_lifetime.Lifetime, part);

            _clock.Tick(10);

            Assert.That(part.Restored, Is.Empty);
        }

        [Test]
        public void EarliestDivergenceAmongPartsAndSourcesWins()
        {
            var late = new ScriptedPart(_order, "late") { Divergence = 9 };
            var early = new ScriptedPart(_order, "early") { Divergence = 7 };
            _stepper.Track(_lifetime.Lifetime, late);
            _stepper.Track(_lifetime.Lifetime, early);
            _stepper.Watch(_lifetime.Lifetime, new ScriptedSource { Divergence = 6 });

            _clock.Tick(10);

            Assert.That(late.Restored, Is.EqualTo(new[] { 6 }));
            Assert.That(early.Restored, Is.EqualTo(new[] { 6 }));
        }

        [Test]
        public void WatchedSourceAloneTriggersAReplay()
        {
            var part = new ScriptedPart(_order, "part");
            _stepper.Track(_lifetime.Lifetime, part);
            _stepper.Watch(_lifetime.Lifetime, new ScriptedSource { Divergence = 8 });

            _clock.Tick(10);

            Assert.That(part.Restored, Is.EqualTo(new[] { 8 }));
        }

        [Test]
        public void PartIsTrackedOnce()
        {
            var part = new ScriptedPart(_order, "part");
            _stepper.Track(_lifetime.Lifetime, part);

            Assert.Throws<InvalidOperationException>(() => _stepper.Track(_lifetime.Lifetime, part));
        }

        [Test]
        public void SourceIsWatchedOnce()
        {
            var source = new ScriptedSource();
            _stepper.Watch(_lifetime.Lifetime, source);

            Assert.Throws<InvalidOperationException>(() => _stepper.Watch(_lifetime.Lifetime, source));
        }

        [Test]
        public void PartLeavesTheRollbackWithItsLifetime()
        {
            var tracked = new LifetimeDefinition();
            var part = new ScriptedPart(_order, "part");
            _stepper.Track(tracked.Lifetime, part);

            tracked.Terminate();
            _clock.Tick(10);

            Assert.That(part.Saved, Is.Empty);
        }

        [Test]
        public void SourceLeavesTheRollbackWithItsLifetime()
        {
            var watched = new LifetimeDefinition();
            var part = new ScriptedPart(_order, "part");
            _stepper.Track(_lifetime.Lifetime, part);
            _stepper.Watch(watched.Lifetime, new ScriptedSource { Divergence = 8 });

            watched.Terminate();
            _clock.Tick(10);

            Assert.That(part.Restored, Is.Empty);
        }

        [Test]
        public void FailingReplayedStepIsLoggedAndTheReplayEnds()
        {
            _stepper.Track(_lifetime.Lifetime, new ScriptedPart(_order, "part") { Divergence = 8 });
            _stepper.Stepped(TickPhase.Default).Advise(_lifetime.Lifetime, _ =>
            {
                if (_stepper.Replaying.Value)
                    throw new InvalidOperationException("replayed step failed");
            });
            LogAssert.Expect(LogType.Exception, new Regex("replayed step failed"));

            _clock.Tick(10);

            Assert.That(_stepper.Replaying.Value, Is.False);
            Assert.That(_order, Has.Member("Publish 10"));
        }

        private sealed class ScriptedPart : IRollbackPart
        {
            public int? Divergence { get; set; }
            public List<int> Saved { get; } = new();
            public List<int> Restored { get; } = new();

            private readonly List<string> _order;
            private readonly string _name;

            public ScriptedPart(List<string> order, string name)
            {
                _order = order;
                _name = name;
            }

            public bool TryGetDivergence(out int tick)
            {
                tick = Divergence ?? default;
                return Divergence.HasValue;
            }

            public void Save(int tick)
            {
                Saved.Add(tick);
                _order.Add($"{_name} saved {tick}");
            }

            public void Follow(int tick) =>
                _order.Add($"{_name} followed {tick}");

            public void Restore(int tick)
            {
                Restored.Add(tick);
                _order.Add($"{_name} restored {tick}");
            }
        }

        private sealed class ScriptedSource : IDivergenceSource
        {
            public int? Divergence { get; set; }

            public bool TryGetDivergence(out int tick)
            {
                tick = Divergence ?? default;
                return Divergence.HasValue;
            }
        }
    }
}
