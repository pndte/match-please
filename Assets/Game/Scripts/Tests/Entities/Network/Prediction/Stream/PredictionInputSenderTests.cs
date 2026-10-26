using System.Linq;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInputSenderTests
    {
        private LifetimeDefinition _lifetime;
        private Toggle _controlled;
        private ScriptedWorldStep _worldStep;
        private CountingSampler _sampler;
        private RecordingRequests<TickedInput<int>> _requests;
        private TickHistory<int> _own;
        private Delay _delay;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _controlled = new Toggle(true);
            _worldStep = new ScriptedWorldStep();
            _sampler = new CountingSampler();
            _requests = new RecordingRequests<TickedInput<int>>();
            _own = new TickHistory<int>(16);
            _delay = new Delay();
            new PredictionInputSender<int>(
                _lifetime.Lifetime,
                _controlled,
                _worldStep,
                _sampler,
                _requests,
                _own,
                _delay,
                new PlusThousandPerTick());
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void EachPrepareSendsOneSampleForItsTick()
        {
            Prepare(100, 104);

            Assert.That(_requests.Requests.Select(request => request.Tick), Is.EqualTo(new[] { 100, 101, 102, 103, 104 }));
            Assert.That(_requests.Requests.Select(request => request.Input), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
            Assert.That(_sampler.Samples, Is.EqualTo(5));
        }

        [Test]
        public void EveryMessageCarriesThePreviousInput()
        {
            Prepare(100, 102);

            Assert.That(_requests.Requests.Select(request => request.PreviousInput), Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void SentInputIsKeptAsTheOwnInputOfItsTick()
        {
            Prepare(100, 101);

            Assert.That(_own.TryGetAt(100, out var first), Is.True);
            Assert.That(_own.TryGetAt(101, out var second), Is.True);
            Assert.That((first, second), Is.EqualTo((1, 2)));
        }

        [Test]
        public void SampleAppliesDelayTicksLater()
        {
            _delay.Set(2);

            _worldStep.Fire(TickPhase.Prepare, 100);

            Assert.That(_requests.Requests.Single().Tick, Is.EqualTo(102));
            Assert.That(_own.TryGetAt(100, out _), Is.False);
            Assert.That(_own.TryGetAt(102, out var input), Is.True);
            Assert.That(input, Is.EqualTo(1));
        }

        [Test]
        public void GrownDelayGivesTheSampleToTheSkippedTickAndItsContinuationToTheNewOne()
        {
            Prepare(100, 104);
            _delay.Set(1);

            _worldStep.Fire(TickPhase.Prepare, 105);

            var grown = _requests.Requests.Skip(5).ToArray();
            Assert.That(grown.Select(request => request.Tick), Is.EqualTo(new[] { 105, 106 }));
            Assert.That(grown.Select(request => request.Input), Is.EqualTo(new[] { 6, 1006 }));
            Assert.That(grown[1].PreviousInput, Is.EqualTo(6));
            Assert.That(_sampler.Samples, Is.EqualTo(6));
            _own.TryGetAt(106, out var continued);
            Assert.That(continued, Is.EqualTo(1006));
        }

        [Test]
        public void ShrunkDelaySkipsATickWithoutSampling()
        {
            _delay.Set(1);
            Prepare(100, 101);
            _delay.Set(0);

            _worldStep.Fire(TickPhase.Prepare, 102);

            Assert.That(_requests.Requests, Has.Count.EqualTo(2));
            Assert.That(_sampler.Samples, Is.EqualTo(2));
        }

        [Test]
        public void SendingGoesOnAfterTheShrunkDelayCatchesUp()
        {
            _delay.Set(1);
            Prepare(100, 101);
            _delay.Set(0);
            _worldStep.Fire(TickPhase.Prepare, 102);

            _worldStep.Fire(TickPhase.Prepare, 103);

            var last = _requests.Requests[^1];
            Assert.That(last.Tick, Is.EqualTo(103));
            Assert.That(last.Input, Is.EqualTo(3));
            Assert.That(last.PreviousInput, Is.EqualTo(2));
        }

        [Test]
        public void ObjectNotControlledHereSendsNothing()
        {
            _controlled.Set(false);

            Prepare(100, 102);

            Assert.That(_requests.Requests, Is.Empty);
            Assert.That(_sampler.Samples, Is.Zero);
        }

        [Test]
        public void ObjectTakenUnderControlStartsSending()
        {
            _controlled.Set(false);
            _worldStep.Fire(TickPhase.Prepare, 100);

            _controlled.Set(true);
            _worldStep.Fire(TickPhase.Prepare, 101);

            Assert.That(_requests.Requests.Single().Tick, Is.EqualTo(101));
        }

        [TestCase(TickPhase.Early)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        [TestCase(TickPhase.Publish)]
        public void OnlyPrepareSamples(TickPhase phase)
        {
            _worldStep.Fire(phase, 100);

            Assert.That(_sampler.Samples, Is.Zero);
            Assert.That(_requests.Requests, Is.Empty);
        }

        [Test]
        public void ReplayNeverSamplesAgain()
        {
            _worldStep.Fire(TickPhase.Prepare, 100);
            _worldStep.SetReplaying(true);

            _worldStep.Fire(TickPhase.Early, 100);
            _worldStep.Fire(TickPhase.Default, 100);
            _worldStep.SetReplaying(false);

            Assert.That(_sampler.Samples, Is.EqualTo(1));
        }

        private void Prepare(int from, int to)
        {
            for (var tick = from; tick <= to; tick++)
                _worldStep.Fire(TickPhase.Prepare, tick);
        }

        private sealed class CountingSampler : IInputSampler<int>
        {
            public int Samples { get; private set; }

            public int Sample() =>
                ++Samples;
        }

        private sealed class Delay : IInputDelay
        {
            public IReadonlyProperty<int> Ticks => _ticks;

            private readonly ViewableProperty<int> _ticks = new(0);

            public void Set(int ticks) =>
                _ticks.Value = ticks;
        }

        private sealed class PlusThousandPerTick : IInputExtrapolator<int>
        {
            public int Continued(int last, int ticks) =>
                last + 1000 * ticks;
        }
    }
}
