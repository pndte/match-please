using System;
using System.Text.RegularExpressions;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class OutcomeReceiverTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private ManualResults<OutcomeReport<float>> _reports;
        private ScriptedWorldStep _worldStep;
        private ServerClock _serverTime;
        private PredictionTargets<Target, float> _targets;
        private OutcomePredictor<Target, float> _predictor;
        private Target _target;
        private RecordingPredictionTarget _part;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _reports = new ManualResults<OutcomeReport<float>>();
            _worldStep = new ScriptedWorldStep();
            _serverTime = new ServerClock();
            _targets = new PredictionTargets<Target, float>();
            _predictor = new OutcomePredictor<Target, float>(_objects.Create("weapon").AddComponent<NetworkObject>(), _targets);
            new OutcomeReceiver<Target, float>(
                _lifetime.Lifetime,
                _reports,
                _worldStep,
                _serverTime,
                _targets,
                new EventPredictionConfig { ExpiryTicks = 15 },
                _predictor);
            _target = new Target();
            _part = new RecordingPredictionTarget();
            _targets.AddLifetimed(_lifetime.Lifetime, _target, 7UL, _part);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void ReportConfirmsTheTargetsOfTheObjectsItNames()
        {
            _predictor.Predict(12, _target, 30f);

            _reports.Deliver(new OutcomeReport<float>(12, new[] { new AffectedTarget<ulong, float>(7UL, 25f) }));

            Assert.That(_part.Calls, Is.EqualTo(new[] { "show 0@12 30", "confirm 0@12 25" }));
            Assert.That(_predictor.Pending.Value, Is.False);
        }

        [Test]
        public void ReportOfAMissRejectsThePrediction()
        {
            _predictor.Predict(12, _target, 30f);

            _reports.Deliver(new OutcomeReport<float>(12, Array.Empty<AffectedTarget<ulong, float>>()));

            Assert.That(_part.Calls[^1], Is.EqualTo("reject 0@12"));
        }

        [Test]
        public void ReportNamingAnObjectWithoutATargetFailsAndAnswersNothing()
        {
            _predictor.Predict(12, _target, 30f);
            LogAssert.Expect(LogType.Exception, new Regex("object 99"));

            _reports.Deliver(new OutcomeReport<float>(12, new[] { new AffectedTarget<ulong, float>(7UL, 25f), new AffectedTarget<ulong, float>(99UL, 5f) }));

            Assert.That(_part.Calls, Is.EqualTo(new[] { "show 0@12 30" }));
            Assert.That(_predictor.Pending.Value, Is.True);
        }

        [Test]
        public void PendingPredictionsOlderThanTheExpiryWindowAreRejectedAtPrepare()
        {
            _predictor.Predict(14, _target, 1f);
            _predictor.Predict(15, _target, 2f);
            _serverTime.Tick = 30;

            _worldStep.Fire(TickPhase.Prepare, 40);

            Assert.That(_part.Calls, Is.EqualTo(new[] { "show 0@14 1", "show 0@15 2", "reject 0@14" }));
        }

        [TestCase(TickPhase.Early)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        [TestCase(TickPhase.Publish)]
        public void OnlyPrepareExpiresPredictions(TickPhase phase)
        {
            _predictor.Predict(1, _target, 1f);
            _serverTime.Tick = 100;

            _worldStep.Fire(phase, 100);

            Assert.That(_part.Calls, Is.EqualTo(new[] { "show 0@1 1" }));
        }

        [Test]
        public void ExpiryCountsFromTheServerTimeNotThePreparedTick()
        {
            _predictor.Predict(14, _target, 1f);
            _serverTime.Tick = 20;

            _worldStep.Fire(TickPhase.Prepare, 1000);

            Assert.That(_part.Calls, Is.EqualTo(new[] { "show 0@14 1" }));
        }

        [Test]
        public void PredictionMadeAfterTheLastAnswerExpiresAgain()
        {
            _predictor.Predict(12, _target, 1f);
            _reports.Deliver(new OutcomeReport<float>(12, Array.Empty<AffectedTarget<ulong, float>>()));
            _predictor.Predict(13, _target, 1f);
            _serverTime.Tick = 100;

            _worldStep.Fire(TickPhase.Prepare, 100);

            Assert.That(_part.Calls[^1], Is.EqualTo("reject 0@13"));
        }

        [Test]
        public void EndedReceiverIgnoresReportsAndExpiry()
        {
            _predictor.Predict(12, _target, 30f);
            _lifetime.Terminate();
            _serverTime.Tick = 100;

            _reports.Deliver(new OutcomeReport<float>(12, Array.Empty<AffectedTarget<ulong, float>>()));
            _worldStep.Fire(TickPhase.Prepare, 100);

            Assert.That(_part.Calls, Is.EqualTo(new[] { "show 0@12 30" }));
        }

        private sealed class ServerClock : IServerTime
        {
            public int Tick { get; set; }
        }

        private sealed class Target
        {
        }
    }
}
