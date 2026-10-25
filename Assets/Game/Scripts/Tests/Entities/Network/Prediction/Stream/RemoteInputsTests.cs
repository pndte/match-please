using System;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class RemoteInputsTests
    {
        private LifetimeDefinition _lifetime;
        private ManualResults<TickedInput<int>> _relayed;
        private Tolerance _divergence;
        private RemoteInputs<int> _inputs;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _relayed = new ManualResults<TickedInput<int>>();
            _divergence = new Tolerance();
            _inputs = new RemoteInputs<int>(_lifetime.Lifetime, _relayed, new PlusThousandPerTick(), _divergence, new WorldRollbackConfig { MaxReplayTicks = 8 });
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void NothingRelayedMeansNoInput()
        {
            var taken = _inputs.TryTakeInputAt(10, out var input);

            Assert.That(taken, Is.False);
            Assert.That(input, Is.Zero);
        }

        [Test]
        public void RelayedTickIsTakenAsRelayed()
        {
            Relay(10, 100, 90);

            Assert.That(_inputs.TryTakeInputAt(10, out var input), Is.True);
            Assert.That(input, Is.EqualTo(100));
        }

        [Test]
        public void TickAfterTheNewestRelayedContinuesIt()
        {
            Relay(10, 100, 90);

            _inputs.TryTakeInputAt(12, out var input);

            Assert.That(input, Is.EqualTo(2100));
        }

        [Test]
        public void TickBeforeEveryRelayedHasNoInput()
        {
            Relay(10, 100, 90);

            Assert.That(_inputs.TryTakeInputAt(9, out _), Is.False);
        }

        [Test]
        public void FirstRelayKeepsOnlyItsOwnTick()
        {
            Relay(10, 100, 90);

            Assert.That(_inputs.TryTakeInputAt(9, out _), Is.False);
        }

        [Test]
        public void RelayOfAnOldOrKnownTickIsIgnored()
        {
            Relay(10, 100, 90);

            Relay(10, 111, 90);
            Relay(9, 99, 88);

            _inputs.TryTakeInputAt(10, out var input);
            Assert.That(input, Is.EqualTo(100));
            Assert.That(_inputs.TryTakeInputAt(9, out _), Is.False);
        }

        [Test]
        public void GapBeforeARelayIsFilledByItsPreviousInput()
        {
            Relay(10, 100, 90);

            Relay(13, 130, 120);

            _inputs.TryTakeInputAt(11, out var guessed);
            _inputs.TryTakeInputAt(12, out var previous);
            Assert.That(guessed, Is.EqualTo(1100));
            Assert.That(previous, Is.EqualTo(120));
        }

        [Test]
        public void NextRelayDoesNotRewriteItsPreviousTick()
        {
            Relay(10, 100, 90);

            Relay(11, 110, 105);

            _inputs.TryTakeInputAt(10, out var input);
            Assert.That(input, Is.EqualTo(100));
        }

        [Test]
        public void NothingTakenIsNoDivergence()
        {
            Relay(10, 100, 90);
            Relay(11, 110, 100);

            Assert.That(_inputs.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void RelayEqualToTheGuessIsNoDivergence()
        {
            Relay(10, 100, 90);
            Take(10, 11);

            Relay(11, 1100, 100);

            Assert.That(_inputs.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void RelayUnlikeTheGuessDivergesFromTheTickBeforeIt()
        {
            Relay(10, 100, 90);
            Take(10, 12);

            Relay(11, 0, 100);

            Assert.That(_inputs.TryGetDivergence(out var tick), Is.True);
            Assert.That(tick, Is.EqualTo(10));
        }

        [Test]
        public void TakingTheRelayedInputSettlesTheDivergence()
        {
            Relay(10, 100, 90);
            Take(10, 12);
            Relay(11, 0, 100);

            Take(11, 12);

            Assert.That(_inputs.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void EarliestDifferenceIsTheDivergence()
        {
            Relay(10, 100, 90);
            Take(10, 13);

            Relay(13, 3, 2);
            Relay(14, 4, 3);

            Assert.That(_inputs.TryGetDivergence(out var tick), Is.True);
            Assert.That(tick, Is.EqualTo(11));
        }

        [Test]
        public void TicksNotTakenYetNeverDiverge()
        {
            Relay(10, 100, 90);
            Take(10, 11);

            Relay(12, 5, 1100);
            Relay(13, 6, 5);

            Assert.That(_inputs.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void DivergenceRuleDecidesWhatDiffers()
        {
            _divergence.Allowed = 5;
            Relay(10, 100, 90);
            Take(10, 11);

            Relay(11, 1104, 100);

            Assert.That(_inputs.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void TakenTicksOlderThanTheReplayWindowAreForgotten()
        {
            Relay(10, 100, 90);
            Take(10, 11);
            Relay(11, 0, 100);

            Take(12, 30);

            Assert.That(_inputs.TryGetDivergence(out _), Is.False);
        }

        [Test]
        public void EndedInputsHearNoRelay()
        {
            _lifetime.Terminate();

            Relay(10, 100, 90);

            Assert.That(_inputs.TryTakeInputAt(10, out _), Is.False);
        }

        private void Relay(int tick, int input, int previous) =>
            _relayed.Deliver(new TickedInput<int>(tick, input, previous));

        private void Take(int from, int to)
        {
            for (var tick = from; tick <= to; tick++)
                _inputs.TryTakeInputAt(tick, out _);
        }

        private sealed class Tolerance : IInputDivergence<int>
        {
            public int Allowed { get; set; }

            public bool Diverged(int guessed, int relayed) =>
                Math.Abs(guessed - relayed) > Allowed;
        }

        private sealed class PlusThousandPerTick : IInputContinuation<int>
        {
            public int Continued(int last, int ticks) =>
                last + 1000 * ticks;
        }
    }
}
