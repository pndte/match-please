using System;
using System.Text.RegularExpressions;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class OutcomeDispatcherTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private Weapon _weapon;
        private PlayerDrivers _drivers;
        private Affectables<Target, float> _affectables;
        private ScriptedOutcomes _outcomes;
        private RecordingResults<OutcomeReport<float>> _reports;
        private IPlayer _shooter;
        private IClient _shooterClient;
        private Target _target;
        private RecordingAffectable<float> _targetPart;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _weapon = new Weapon();
            _drivers = new PlayerDrivers();
            _affectables = new Affectables<Target, float>();
            _outcomes = new ScriptedOutcomes();
            _reports = new RecordingResults<OutcomeReport<float>>();
            _shooter = new Player("shooter");
            _shooterClient = new Client(default);
            _drivers.AddClientDriven(_lifetime.Lifetime, _shooter, _shooterClient);
            new OutcomeDispatcher<Weapon, Target, float>(
                _lifetime.Lifetime,
                _weapon,
                _objects.Create("weapon").AddComponent<NetworkObject>(),
                _drivers,
                _affectables,
                _outcomes,
                _reports);
            _target = new Target();
            _targetPart = new RecordingAffectable<float>();
            _affectables.AddLifetimed(_lifetime.Lifetime, _target, 7UL, _targetPart);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void OutcomeAffectsItsTargetUnderTheActionOfItsTick()
        {
            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f)));

            Assert.That(_targetPart.Affected, Has.Count.EqualTo(1));
            Assert.That(StateCauseTests.Described(_targetPart.Affected[0].Cause), Is.EqualTo("0@12"));
            Assert.That(_targetPart.Affected[0].Effect, Is.EqualTo(25f));
        }

        [Test]
        public void EveryTargetGetsItsOwnEffect()
        {
            var other = new Target();
            var otherPart = new RecordingAffectable<float>();
            _affectables.AddLifetimed(_lifetime.Lifetime, other, 8UL, otherPart);

            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f), (other, 10f)));

            Assert.That(_targetPart.Affected[0].Effect, Is.EqualTo(25f));
            Assert.That(otherPart.Affected[0].Effect, Is.EqualTo(10f));
        }

        [Test]
        public void ActorsClientGetsOneReportWithTheTargetsObjects()
        {
            var other = new Target();
            _affectables.AddLifetimed(_lifetime.Lifetime, other, 8UL, new RecordingAffectable<float>());

            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f), (other, 10f)));

            Assert.That(_reports.Sent, Has.Count.EqualTo(1));
            Assert.That(_reports.Sent[0].Recipient, Is.SameAs(_shooterClient));
            var report = _reports.Sent[0].Result;
            Assert.That(report.Tick, Is.EqualTo(12));
            Assert.That(report.Affected, Is.EqualTo(new[] { new AffectedTarget<ulong, float>(7UL, 25f), new AffectedTarget<ulong, float>(8UL, 10f) }));
            Assert.That(_reports.Broadcasts, Is.Empty);
        }

        [Test]
        public void MissIsReportedWithNoTargets()
        {
            _outcomes.Report(Outcome(_shooter, 12));

            Assert.That(_reports.Sent, Has.Count.EqualTo(1));
            Assert.That(_reports.Sent[0].Result.Affected, Is.Empty);
        }

        [Test]
        public void OutcomeOfAServerDrivenActorAffectsWithoutAReport()
        {
            var bot = new Player("bot");
            _drivers.AddServerDriven(_lifetime.Lifetime, bot);

            _outcomes.Report(Outcome(bot, 12, (_target, 25f)));

            Assert.That(_targetPart.Affected, Has.Count.EqualTo(1));
            Assert.That(_reports.Sent, Is.Empty);
            Assert.That(_reports.Broadcasts, Is.Empty);
        }

        [Test]
        public void OutcomeOfAnotherInitiatorIsLeftToItsOwnDispatcher()
        {
            _outcomes.Report(new ActionOutcome<Weapon, Target, float>(new Weapon(), _shooter, 12, new[] { new AffectedTarget<Target, float>(_target, 25f) }));

            Assert.That(_targetPart.Affected, Is.Empty);
            Assert.That(_reports.Sent, Is.Empty);
        }

        [Test]
        public void InitiatorActsOncePerTick()
        {
            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f)));
            LogAssert.Expect(LogType.Exception, new Regex("once per tick"));

            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f)));

            Assert.That(_targetPart.Affected, Has.Count.EqualTo(1));
            Assert.That(_reports.Sent, Has.Count.EqualTo(1));
        }

        [Test]
        public void OutcomeOfAnEarlierTickIsRefused()
        {
            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f)));
            LogAssert.Expect(LogType.Exception, new Regex("once per tick"));

            _outcomes.Report(Outcome(_shooter, 11, (_target, 25f)));

            Assert.That(_targetPart.Affected, Has.Count.EqualTo(1));
        }

        [Test]
        public void LaterTicksKeepComing()
        {
            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f)));
            _outcomes.Report(Outcome(_shooter, 13, (_target, 5f)));
            _outcomes.Report(Outcome(_shooter, 40));

            Assert.That(_targetPart.Affected, Has.Count.EqualTo(2));
            Assert.That(_reports.Sent, Has.Count.EqualTo(3));
        }

        [Test]
        public void TargetNamedTwiceAffectsNothing()
        {
            LogAssert.Expect(LogType.Exception, new Regex("each target once"));

            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f), (_target, 5f)));

            Assert.That(_targetPart.Affected, Is.Empty);
            Assert.That(_reports.Sent, Is.Empty);
        }

        [Test]
        public void UnregisteredTargetAffectsNothingAtAll()
        {
            LogAssert.Expect(LogType.Exception, new Regex("not registered as affectable"));

            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f), (new Target(), 5f)));

            Assert.That(_targetPart.Affected, Is.Empty);
            Assert.That(_reports.Sent, Is.Empty);
        }

        [Test]
        public void RefusedOutcomeLeavesItsTickFree()
        {
            LogAssert.Expect(LogType.Exception, new Regex("not registered as affectable"));
            _outcomes.Report(Outcome(_shooter, 12, (new Target(), 5f)));

            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f)));

            Assert.That(_targetPart.Affected, Has.Count.EqualTo(1));
        }

        [Test]
        public void OutcomeOfAnActorWithoutADriverAffectsNothing()
        {
            LogAssert.Expect(LogType.Exception, new Regex("has no driver"));

            _outcomes.Report(Outcome(new Player("gone"), 12, (_target, 25f)));

            Assert.That(_targetPart.Affected, Is.Empty);
        }

        [Test]
        public void EndedDispatcherIgnoresOutcomes()
        {
            var affectables = new Affectables<Target, float>();
            var part = new RecordingAffectable<float>();
            affectables.AddLifetimed(_lifetime.Lifetime, _target, 7UL, part);
            var ended = new LifetimeDefinition();
            new OutcomeDispatcher<Weapon, Target, float>(
                ended.Lifetime,
                _weapon,
                _objects.Create("other weapon").AddComponent<NetworkObject>(),
                _drivers,
                affectables,
                _outcomes,
                _reports);

            ended.Terminate();
            _outcomes.Report(Outcome(_shooter, 12, (_target, 25f)));

            Assert.That(part.Affected, Is.Empty);
        }

        private ActionOutcome<Weapon, Target, float> Outcome(IPlayer actor, int tick, params (Target Target, float Effect)[] affected) =>
            new(_weapon, actor, tick, Array.ConvertAll(affected, entry => new AffectedTarget<Target, float>(entry.Target, entry.Effect)));

        private sealed class ScriptedOutcomes : IOutcomes<Weapon, Target, float>
        {
            public ISource<ActionOutcome<Weapon, Target, float>> Reported => _reported;

            private readonly Signal<ActionOutcome<Weapon, Target, float>> _reported = new();

            public void Report(ActionOutcome<Weapon, Target, float> outcome) =>
                _reported.Fire(outcome);
        }

        private sealed class Weapon
        {
        }

        private sealed class Target
        {
        }
    }
}
