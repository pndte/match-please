using System.Collections;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class PredictionInterpolationTests
    {
        private const int MaxFrames = 10;

        private LifetimeDefinition _lifetime;
        private Toggle _predicted;
        private ScriptedInterpolationTicks _ticks;
        private SteppedSimulation<float> _simulation;
        private RecordingView<float> _view;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _predicted = new Toggle();
            _ticks = new ScriptedInterpolationTicks();
            _simulation = new SteppedSimulation<float>(3f);
            _view = new RecordingView<float>();
            new PredictionInterpolation<float>(_lifetime.Lifetime, _predicted, _ticks, _simulation, _view);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [UnityTest]
        public IEnumerator PredictedObjectShowsItsLastStepByTheTickProgress()
        {
            _simulation.Step(5f);
            _ticks.Progress = 0.25f;
            _predicted.Set(true);

            for (var frame = 0; frame < MaxFrames && _view.Shown.Count == 0; frame++)
                yield return null;

            Assert.That(_view.Shown, Is.Not.Empty);
            Assert.That(_view.Shown[^1], Is.EqualTo((3f, 5f, 0.25f)));
        }

        [UnityTest]
        public IEnumerator ObjectNotPredictedIsNotShownByTheSimulation()
        {
            _simulation.Step(5f);

            for (var frame = 0; frame < MaxFrames; frame++)
                yield return null;

            Assert.That(_view.Shown, Is.Empty);
        }

        [UnityTest]
        public IEnumerator LeavingThePredictionStopsTheView()
        {
            _predicted.Set(true);
            for (var frame = 0; frame < MaxFrames && _view.Shown.Count == 0; frame++)
                yield return null;

            _predicted.Set(false);
            var shown = _view.Shown.Count;
            for (var frame = 0; frame < MaxFrames; frame++)
                yield return null;

            Assert.That(shown, Is.GreaterThan(0));
            Assert.That(_view.Shown.Count, Is.EqualTo(shown));
        }
    }
}
