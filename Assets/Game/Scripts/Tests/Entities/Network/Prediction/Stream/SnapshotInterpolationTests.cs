using System.Collections;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class SnapshotInterpolationTests
    {
        private const int MaxFrames = 10;

        private LifetimeDefinition _lifetime;
        private Toggle _predicted;
        private ScriptedInterpolationTicks _ticks;
        private ServerStates<float> _server;
        private RecordingView<float> _view;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _predicted = new Toggle();
            _ticks = new ScriptedInterpolationTicks();
            _server = new ServerStates<float>(16);
            _view = new RecordingView<float>();
            new SnapshotInterpolation<float>(_lifetime.Lifetime, _predicted, _ticks, _server, _view);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [UnityTest]
        public IEnumerator ViewBlendsTheTwoServerStatesAroundTheInterpolationTick()
        {
            _server.Record(10, 100f);
            _server.Record(12, 120f);
            _ticks.InterpolationTick = 11.5;

            for (var frame = 0; frame < MaxFrames && _view.Shown.Count == 0; frame++)
                yield return null;

            Assert.That(_view.Shown, Is.Not.Empty);
            var (from, to, progress) = _view.Shown[^1];
            Assert.That((from, to), Is.EqualTo((100f, 120f)));
            Assert.That(progress, Is.EqualTo(0.75f).Within(1e-5f));
        }

        [UnityTest]
        public IEnumerator ViewAtAServerTickShowsThatState()
        {
            _server.Record(10, 100f);
            _server.Record(11, 110f);
            _ticks.InterpolationTick = 10.0;

            for (var frame = 0; frame < MaxFrames && _view.Shown.Count == 0; frame++)
                yield return null;

            Assert.That(_view.Shown[^1], Is.EqualTo((100f, 110f, 0f)));
        }

        [UnityTest]
        public IEnumerator ViewPastTheNewestServerStateHoldsIt()
        {
            _server.Record(10, 100f);
            _server.Record(12, 120f);
            _ticks.InterpolationTick = 15.3;

            for (var frame = 0; frame < MaxFrames && _view.Shown.Count == 0; frame++)
                yield return null;

            Assert.That(_view.Shown[^1], Is.EqualTo((120f, 120f, 0f)));
        }

        [UnityTest]
        public IEnumerator ViewBeforeTheFirstServerStateShowsTheFirst()
        {
            _server.Record(10, 100f);
            _server.Record(12, 120f);
            _ticks.InterpolationTick = 8.5;

            for (var frame = 0; frame < MaxFrames && _view.Shown.Count == 0; frame++)
                yield return null;

            Assert.That(_view.Shown[^1], Is.EqualTo((100f, 100f, 0f)));
        }

        [UnityTest]
        public IEnumerator NothingFromTheServerShowsNothing()
        {
            _ticks.InterpolationTick = 10.0;

            for (var frame = 0; frame < MaxFrames; frame++)
                yield return null;

            Assert.That(_view.Shown, Is.Empty);
        }

        [UnityTest]
        public IEnumerator PredictedObjectIsNotInterpolated()
        {
            _predicted.Set(true);
            _server.Record(10, 100f);
            _ticks.InterpolationTick = 10.0;

            for (var frame = 0; frame < MaxFrames; frame++)
                yield return null;

            Assert.That(_view.Shown, Is.Empty);
        }
    }
}
