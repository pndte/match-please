using System.Text.RegularExpressions;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class InputGapLogTests
    {
        private const int Interval = 3;

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private ManualClock _clock;
        private ManualResults<TickedInput<int>> _requests;
        private PredictionInputBuffer<int> _buffer;
        private ScriptedWorldStep _worldStep;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _clock = new ManualClock();
            _requests = new ManualResults<TickedInput<int>>();
            var config = new NetworkTicksConfig { MaxBufferedInputTicks = 4, GapReportIntervalTicks = Interval };
            _buffer = new PredictionInputBuffer<int>(_lifetime.Lifetime, _clock, config, new AnyInput(), new SameInput(), _requests);
            _worldStep = new ScriptedWorldStep();
            new InputGapLog<int>(_lifetime.Lifetime, _worldStep, config, _buffer, _objects.Create("crate").AddComponent<NetworkObject>());
            _clock.Tick(5);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void WindowWithoutGapsLogsNothing()
        {
            Hear(6, 60, 59);
            _clock.Tick(6);
            _buffer.TryTakeInputAt(6, out _);

            Publish(Interval);

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void WindowWithGapsLogsEveryKindOnce()
        {
            Hear(5, 50, 49);
            Hear(4, 40, 39);
            Hear(20, 200, 190);
            Hear(6, 60, 59);
            Hear(8, 80, 70);
            _buffer.TryTakeInputAt(9, out _);
            LogAssert.Expect(LogType.Warning, Exactly(
                "[Input] crate #0 Int32, last 3 ticks: 2 inputs came after their tick and were dropped (1.5 ticks late on average), "
                + "1 came more than 4 ticks early and were dropped, 1 missing inputs came from the next packet's copy, 1 ticks ran on a repeated input."));

            Publish(Interval);
        }

        [Test]
        public void WindowWithoutLateInputsLeavesTheAverageOut()
        {
            Hear(20, 200, 190);
            LogAssert.Expect(LogType.Warning, Exactly(
                "[Input] crate #0 Int32, last 3 ticks: 0 inputs came after their tick and were dropped, "
                + "1 came more than 4 ticks early and were dropped, 0 missing inputs came from the next packet's copy, 0 ticks ran on a repeated input."));

            Publish(Interval);
        }

        [Test]
        public void ReportWaitsForTheWholeWindow()
        {
            Hear(20, 200, 190);

            Publish(Interval - 1);

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void CountsStartOverAfterAReport()
        {
            Hear(20, 200, 190);
            LogAssert.Expect(LogType.Warning, new Regex("came more than"));
            Publish(Interval);

            Publish(Interval);

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void QuietTicksCountTowardsTheWindow()
        {
            Publish(Interval - 1);
            Hear(20, 200, 190);
            LogAssert.Expect(LogType.Warning, new Regex("came more than"));

            Publish(1);
        }

        private void Hear(int tick, int input, int previous) =>
            _requests.Deliver(new TickedInput<int>(tick, input, previous));

        private void Publish(int ticks)
        {
            for (var tick = 0; tick < ticks; tick++)
                _worldStep.Fire(TickPhase.Publish, tick);
        }

        private static Regex Exactly(string message) =>
            new("^" + Regex.Escape(message) + "$");

        private sealed class AnyInput : IInputPolicy<int>
        {
            public bool IsValid(int input) =>
                true;

            public int CaughtUp(
                int input,
                int lastInput,
                int heard,
                int heardTicksAgo) =>
                input;
        }

        private sealed class SameInput : IInputContinuation<int>
        {
            public int Continued(int last, int ticks) =>
                last;
        }
    }
}
