using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputBufferTests
    {
        private const int MaxBufferedTicks = 4;

        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private ManualResults<TickedInput<int>> _requests;
        private Policy _policy;
        private PredictionInputBuffer<int> _buffer;
        private List<InputGap> _gaps;
        private List<int> _stored;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock();
            _requests = new ManualResults<TickedInput<int>>();
            _policy = new Policy();
            _buffer = new PredictionInputBuffer<int>(
                _lifetime.Lifetime,
                _clock,
                new NetworkTicksConfig { MaxBufferedInputTicks = MaxBufferedTicks },
                _policy,
                new PlusThousandPerTick(),
                _requests);
            _gaps = new List<InputGap>();
            _stored = new List<int>();
            _buffer.Gaps.Advise(_lifetime.Lifetime, _gaps.Add);
            _buffer.Stored.Advise(_lifetime.Lifetime, request => _stored.Add(request.Tick));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void NothingHeardMeansNoInput()
        {
            var taken = _buffer.TryTakeInputAt(1, out var input);

            Assert.That(taken, Is.False);
            Assert.That(input, Is.Zero);
            Assert.That(_gaps, Is.Empty);
        }

        [Test]
        public void StoredInputIsTakenAtItsTick()
        {
            Hear(3, 30, 29);

            var taken = _buffer.TryTakeInputAt(3, out var input);

            Assert.That(taken, Is.True);
            Assert.That(input, Is.EqualTo(30));
            Assert.That(_policy.CaughtUp, Is.EqualTo(new[] { (30, 0, 30, 0) }));
        }

        [Test]
        public void StoredInputIsAnnounced()
        {
            Hear(3, 30, 29);

            Assert.That(_stored, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void TickBeforeTheFirstHeardInputHasNoInput()
        {
            Hear(3, 30, 29);

            Assert.That(_buffer.TryTakeInputAt(2, out _), Is.False);
            Assert.That(_gaps, Is.Empty);
        }

        [TestCase(5, 5, 1)]
        [TestCase(5, 3, 3)]
        [TestCase(5, -10, 16)]
        public void LateInputIsDroppedAndReported(int current, int tick, int lateTicks)
        {
            _clock.Tick(current);

            Hear(tick, 10, 9);

            Assert.That(_stored, Is.Empty);
            Assert.That(_gaps, Has.Count.EqualTo(1));
            Assert.That(_gaps[0].Kind, Is.EqualTo(InputGapKind.Late));
            Assert.That(_gaps[0].LateTicks, Is.EqualTo(lateTicks));
            Assert.That(_buffer.TryTakeInputAt(tick, out _), Is.False);
        }

        [Test]
        public void InputBeyondTheBufferIsDroppedAndReported()
        {
            Hear(MaxBufferedTicks + 1, 10, 9);

            Assert.That(_stored, Is.Empty);
            Assert.That(_gaps, Has.Count.EqualTo(1));
            Assert.That(_gaps[0].Kind, Is.EqualTo(InputGapKind.Early));
        }

        [Test]
        public void InputAtTheFarEdgeOfTheBufferIsStored()
        {
            Hear(MaxBufferedTicks, 10, 9);

            Assert.That(_stored, Is.EqualTo(new[] { MaxBufferedTicks }));
            Assert.That(_gaps, Is.Empty);
        }

        [Test]
        public void InvalidInputIsNeitherStoredNorHeard()
        {
            Hear(3, -1, 0);

            Assert.That(_stored, Is.Empty);
            Assert.That(_gaps, Is.Empty);
            Assert.That(_buffer.TryTakeInputAt(3, out _), Is.False);
        }

        [Test]
        public void RepeatedInputKeepsTheFirstValue()
        {
            Hear(3, 30, 29);

            Hear(3, 31, 29);

            Assert.That(_stored, Is.EqualTo(new[] { 3 }));
            _buffer.TryTakeInputAt(3, out var input);
            Assert.That(input, Is.EqualTo(30));
        }

        [Test]
        public void PreviousInputFillsAMissingTick()
        {
            _clock.Tick(1);
            Hear(3, 30, 29);

            Hear(5, 50, 40);

            Assert.That(_gaps, Has.Count.EqualTo(1));
            Assert.That(_gaps[0].Kind, Is.EqualTo(InputGapKind.Recovered));
            _buffer.TryTakeInputAt(4, out var input);
            Assert.That(input, Is.EqualTo(40));
        }

        [Test]
        public void PreviousInputNeverReachesBeforeTheFirstHeardTick()
        {
            Hear(3, 30, 29);

            Assert.That(_buffer.TryTakeInputAt(2, out _), Is.False);
            Assert.That(_gaps, Is.Empty);
        }

        [Test]
        public void PreviousInputOfAStoredTickIsNotStoredAgain()
        {
            Hear(3, 30, 29);
            Hear(4, 40, 30);

            Assert.That(_gaps, Is.Empty);
        }

        [Test]
        public void OlderInputMovesTheFirstTickBack()
        {
            Hear(3, 30, 29);

            Hear(2, 20, 19);

            Assert.That(_buffer.TryTakeInputAt(2, out var input), Is.True);
            Assert.That(input, Is.EqualTo(20));
        }

        [Test]
        public void MissingTickAfterTheNewestContinuesTheNewestHeardInput()
        {
            Hear(3, 30, 29);
            _buffer.TryTakeInputAt(3, out _);

            var taken = _buffer.TryTakeInputAt(5, out var input);

            Assert.That(taken, Is.True);
            Assert.That(input, Is.EqualTo(2030));
            Assert.That(_policy.CaughtUp[^1], Is.EqualTo((2030, 30, 30, 2)));
            Assert.That(_gaps[^1].Kind, Is.EqualTo(InputGapKind.Substituted));
        }

        [Test]
        public void MissingTickBeforeTheNewestContinuesTheLastKnownInputByOneTick()
        {
            Hear(3, 30, 29);
            Hear(6, 60, 60);
            _buffer.TryTakeInputAt(3, out _);

            _buffer.TryTakeInputAt(4, out var input);

            Assert.That(input, Is.EqualTo(1030));
            Assert.That(_policy.CaughtUp[^1], Is.EqualTo((1030, 30, 60, -2)));
        }

        [Test]
        public void LateInputStillReachesThePolicyAsTheNewestHeard()
        {
            Hear(3, 30, 29);
            _clock.Tick(3);
            _buffer.TryTakeInputAt(3, out _);
            _clock.Tick(4);

            Hear(4, 44, 33);
            _buffer.TryTakeInputAt(4, out var input);

            Assert.That(input, Is.EqualTo(1030));
            Assert.That(_policy.CaughtUp[^1], Is.EqualTo((1030, 30, 44, 0)));
        }

        [Test]
        public void InvalidInputIsNeverTheNewestHeard()
        {
            Hear(3, 30, 29);
            _buffer.TryTakeInputAt(3, out _);

            Hear(4, -5, 30);
            _buffer.TryTakeInputAt(5, out _);

            Assert.That(_policy.CaughtUp[^1].Heard, Is.EqualTo(30));
        }

        [Test]
        public void PolicyGetsTheLastInputTheSimulationTook()
        {
            _policy.Bonus = 7;
            Hear(3, 30, 29);
            Hear(4, 40, 30);

            _buffer.TryTakeInputAt(3, out var first);
            _buffer.TryTakeInputAt(4, out var second);

            Assert.That(first, Is.EqualTo(37));
            Assert.That(second, Is.EqualTo(47));
            Assert.That(_policy.CaughtUp[^1].Last, Is.EqualTo(37));
        }

        [Test]
        public void ClearForgetsEveryInput()
        {
            Hear(3, 30, 29);
            Hear(4, 40, 30);

            _buffer.Clear();

            Assert.That(_buffer.TryTakeInputAt(3, out _), Is.False);
            Assert.That(_buffer.TryTakeInputAt(4, out _), Is.False);
        }

        [Test]
        public void ClearStartsTheNextControllerFromNothing()
        {
            Hear(3, 30, 29);
            _buffer.TryTakeInputAt(3, out _);
            _buffer.Clear();

            Hear(4, 40, 39);
            _buffer.TryTakeInputAt(4, out _);

            Assert.That(_policy.CaughtUp[^1], Is.EqualTo((40, 0, 40, 0)));
        }

        [Test]
        public void EndedBufferHearsNothing()
        {
            _lifetime.Terminate();

            Hear(3, 30, 29);

            Assert.That(_buffer.TryTakeInputAt(3, out _), Is.False);
        }

        private void Hear(int tick, int input, int previous) =>
            _requests.Deliver(new TickedInput<int>(tick, input, previous));

        private sealed class Policy : IInputPolicy<int>
        {
            public List<(int Input, int Last, int Heard, int Ago)> CaughtUp { get; } = new();
            public int Bonus { get; set; }

            public bool IsValid(int input) =>
                input >= 0;

            int IInputPolicy<int>.CaughtUp(
                int input,
                int lastInput,
                int heard,
                int heardTicksAgo)
            {
                CaughtUp.Add((input, lastInput, heard, heardTicksAgo));
                return input + Bonus;
            }
        }

        private sealed class PlusThousandPerTick : IInputExtrapolator<int>
        {
            public int Continued(int last, int ticks) =>
                last + 1000 * ticks;
        }
    }
}
