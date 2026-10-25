using System;
using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class PredictedStateTests
    {
        private static readonly ActionId First = new(5UL, 10);
        private static readonly ActionId Second = new(5UL, 11);

        private LifetimeDefinition _lifetime;
        private ManualResults<CausedState<int>> _server;
        private AppliedValue<int> _state;
        private HealthRules _rules;
        private PredictedState<int, int> _predicted;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _server = new ManualResults<CausedState<int>>();
            _state = new AppliedValue<int>(100);
            _rules = new HealthRules();
            _predicted = new PredictedState<int, int>(_lifetime.Lifetime, _server, _state, _rules);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ShownEffectIsPresentedAsPredicted()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            Assert.That(_state.Current, Is.EqualTo(70));
            Assert.That(_rules.Presented, Is.EqualTo(new[] { "100 to 70 predicted 30" }));
        }

        [Test]
        public void ShownEffectsAddUp()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));
            _predicted.Show(new PredictedEffect<int>(Second, 20));

            Assert.That(_state.History, Is.EqualTo(new[] { 70, 50 }));
        }

        [Test]
        public void PredictionStartsFromTheStateItWasMadeWith()
        {
            var state = new AppliedValue<int>(40);
            var predicted = new PredictedState<int, int>(_lifetime.Lifetime, new ManualResults<CausedState<int>>(), state, new HealthRules());

            predicted.Show(new PredictedEffect<int>(First, 15));

            Assert.That(state.Current, Is.EqualTo(25));
        }

        [Test]
        public void ConfirmReplacesThePredictionWithTheRealEffectSilently()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            _predicted.Confirm(First, 25);

            Assert.That(_state.Current, Is.EqualTo(75));
            Assert.That(_rules.Presented[^1], Is.EqualTo("70 to 75 correction"));
        }

        [Test]
        public void ConfirmOfAnActionNotWaitingChangesNothing()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            _predicted.Confirm(Second, 25);

            Assert.That(_state.History, Is.EqualTo(new[] { 70 }));
        }

        [Test]
        public void RejectWithdrawsThePredictionSilently()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            _predicted.Reject(First);

            Assert.That(_state.Current, Is.EqualTo(100));
            Assert.That(_rules.Presented[^1], Is.EqualTo("70 to 100 correction"));
        }

        [Test]
        public void RejectOfAnActionNotWaitingChangesNothing()
        {
            _predicted.Reject(First);

            Assert.That(_state.History, Is.Empty);
        }

        [Test]
        public void SecondAnswerForTheSameActionChangesNothing()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));
            _predicted.Confirm(First, 25);

            _predicted.Confirm(First, 10);
            _predicted.Reject(First);

            Assert.That(_state.History, Is.EqualTo(new[] { 70, 75 }));
        }

        [Test]
        public void EffectsOfOneActionAreConfirmedAsOneRealEffect()
        {
            _predicted.Show(new PredictedEffect<int>(First, 10));
            _predicted.Show(new PredictedEffect<int>(First, 10));

            _predicted.Confirm(First, 15);

            Assert.That(_state.Current, Is.EqualTo(85));
        }

        [Test]
        public void ConfirmedEffectStaysUntilTheStateItCausedArrives()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));
            _predicted.Confirm(First, 25);

            _server.Deliver(new CausedState<int>(100, StateCause.None));

            Assert.That(_state.Current, Is.EqualTo(75));
        }

        [Test]
        public void ServerStateCausedByTheActionSettlesItsConfirmedEffect()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));
            _predicted.Confirm(First, 25);

            _server.Deliver(new CausedState<int>(75, StateCause.Of(First)));
            _server.Deliver(new CausedState<int>(75, StateCause.None));

            Assert.That(_state.History, Is.EqualTo(new[] { 70, 75, 75, 75 }));
            Assert.That(_rules.Presented[2], Is.EqualTo("75 to 75 authoritative 75"));
        }

        [Test]
        public void ServerStateBeforeTheReportSettlesThePendingEffect()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            _server.Deliver(new CausedState<int>(75, StateCause.Of(First)));
            _predicted.Confirm(First, 25);

            Assert.That(_state.History, Is.EqualTo(new[] { 70, 75 }));
        }

        [Test]
        public void UncausedServerStateKeepsEveryWaitingEffect()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            _server.Deliver(new CausedState<int>(90, StateCause.None));

            Assert.That(_state.Current, Is.EqualTo(60));
            Assert.That(_rules.Presented[^1], Is.EqualTo("70 to 60 authoritative 90"));
        }

        [Test]
        public void ServerStateOfAnotherActionKeepsThisOnesEffect()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            _server.Deliver(new CausedState<int>(80, StateCause.Of(Second)));

            Assert.That(_state.Current, Is.EqualTo(50));
        }

        [Test]
        public void ServerStateOfAnotherInitiatorAtTheSameTickKeepsThisOnesEffect()
        {
            _predicted.Show(new PredictedEffect<int>(First, 30));

            _server.Deliver(new CausedState<int>(80, StateCause.Of(new ActionId(6UL, First.Tick))));

            Assert.That(_state.Current, Is.EqualTo(50));
        }

        [TestCase("reports first", "confirm first", "confirm second", "state first", "state second")]
        [TestCase("states first", "state first", "state second", "confirm first", "confirm second")]
        [TestCase("interleaved", "state first", "confirm first", "confirm second", "state second")]
        [TestCase("second report first", "confirm second", "state first", "state second", "confirm first")]
        public void ReportsAndStatesInAnyOrderCountEachEffectOnce(string order, params string[] answers)
        {
            var server = new ManualResults<CausedState<int>>();
            var state = new AppliedValue<int>(100);
            var predicted = new PredictedState<int, int>(_lifetime.Lifetime, server, state, new HealthRules());
            predicted.Show(new PredictedEffect<int>(First, 30));
            predicted.Show(new PredictedEffect<int>(Second, 20));

            foreach (var answer in answers)
                Answer(answer, predicted, server);

            Assert.That(state.Current, Is.EqualTo(55), order);
            Assert.That(state.History, Is.All.GreaterThanOrEqualTo(50), order);
        }

        [Test]
        public void AppliedStateIsWhatTheRulesPresent()
        {
            _rules.Offset = 1000;

            _predicted.Show(new PredictedEffect<int>(First, 30));

            Assert.That(_state.Current, Is.EqualTo(1070));
        }

        [Test]
        public void EndedPredictionIgnoresTheServer()
        {
            _lifetime.Terminate();

            _server.Deliver(new CausedState<int>(10, StateCause.None));

            Assert.That(_state.History, Is.Empty);
        }

        private static void Answer(string answer, PredictedState<int, int> predicted, ManualResults<CausedState<int>> server)
        {
            switch (answer)
            {
                case "confirm first":
                    predicted.Confirm(First, 25);
                    break;
                case "confirm second":
                    predicted.Confirm(Second, 20);
                    break;
                case "state first":
                    server.Deliver(new CausedState<int>(75, StateCause.Of(First)));
                    break;
                case "state second":
                    server.Deliver(new CausedState<int>(55, StateCause.Of(Second)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(answer), answer, null);
            }
        }

        private sealed class HealthRules : IPredictionRules<int, int>
        {
            public List<string> Presented { get; } = new();
            public int Offset { get; set; }

            public int Predict(int state, int effect) =>
                state - effect;

            public int Present(int shown, int next, ChangeCause<int, int> cause)
            {
                var described = cause.Switch(
                    0,
                    static (_, effect) => $"predicted {effect.ToString()}",
                    static (_, state) => $"authoritative {state.ToString()}",
                    static _ => "correction");
                Presented.Add($"{shown.ToString()} to {next.ToString()} {described}");
                return next + Offset;
            }
        }
    }
}
