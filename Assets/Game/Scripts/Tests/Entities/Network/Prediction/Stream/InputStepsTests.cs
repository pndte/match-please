using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class InputStepsTests
    {
        private LifetimeDefinition _lifetime;
        private ScriptedWorldStep _worldStep;
        private ScriptedInputs _inputs;
        private RecordingSteps _simulation;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _worldStep = new ScriptedWorldStep();
            _inputs = new ScriptedInputs();
            _simulation = new RecordingSteps();
            new InputSteps<int>(_lifetime.Lifetime, _worldStep, _inputs, _simulation);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void DefaultStepsTheSimulationWithTheInputOfItsTick()
        {
            _inputs.ByTick[5] = 50;
            _inputs.ByTick[6] = 60;

            _worldStep.Fire(TickPhase.Default, 5);
            _worldStep.Fire(TickPhase.Default, 6);

            Assert.That(_inputs.Asked, Is.EqualTo(new[] { 5, 6 }));
            Assert.That(_simulation.Steps, Is.EqualTo(new[] { 50, 60 }));
        }

        [Test]
        public void TickWithoutAnInputIsNotStepped()
        {
            _inputs.ByTick[6] = 60;

            _worldStep.Fire(TickPhase.Default, 5);
            _worldStep.Fire(TickPhase.Default, 6);

            Assert.That(_simulation.Steps, Is.EqualTo(new[] { 60 }));
        }

        [Test]
        public void ReplayedTickStepsAgainWithItsInput()
        {
            _inputs.ByTick[5] = 50;
            _worldStep.Fire(TickPhase.Default, 5);
            _inputs.ByTick[5] = 51;

            _worldStep.SetReplaying(true);
            _worldStep.Fire(TickPhase.Default, 5);

            Assert.That(_simulation.Steps, Is.EqualTo(new[] { 50, 51 }));
        }

        [TestCase(TickPhase.Prepare)]
        [TestCase(TickPhase.Early)]
        [TestCase(TickPhase.Late)]
        [TestCase(TickPhase.Publish)]
        public void OnlyDefaultSteps(TickPhase phase)
        {
            _inputs.ByTick[5] = 50;

            _worldStep.Fire(phase, 5);

            Assert.That(_inputs.Asked, Is.Empty);
            Assert.That(_simulation.Steps, Is.Empty);
        }

        [Test]
        public void EndedStepsStop()
        {
            _inputs.ByTick[5] = 50;
            _lifetime.Terminate();

            _worldStep.Fire(TickPhase.Default, 5);

            Assert.That(_simulation.Steps, Is.Empty);
        }

        private sealed class ScriptedInputs : IStepInputs<int>
        {
            public Dictionary<int, int> ByTick { get; } = new();
            public List<int> Asked { get; } = new();

            public bool TryTakeInputAt(int tick, out int input)
            {
                Asked.Add(tick);
                return ByTick.TryGetValue(tick, out input);
            }
        }

        private sealed class RecordingSteps : ISteppable<int>
        {
            public List<int> Steps { get; } = new();

            public void Step(int input) =>
                Steps.Add(input);
        }
    }
}
