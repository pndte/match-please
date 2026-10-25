using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class StateCorrectionsTests
    {
        private LifetimeDefinition _lifetime;
        private SteppedSimulation<int> _simulation;
        private ScriptedWorldStep _worldStep;
        private List<StateCorrection<int>> _corrections;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _simulation = new SteppedSimulation<int>(5);
            _worldStep = new ScriptedWorldStep();
            _corrections = new List<StateCorrection<int>>();
            new StateCorrections<int>(_lifetime.Lifetime, _simulation, _worldStep).Corrected.Advise(_lifetime.Lifetime, _corrections.Add);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ReplayEndingElsewhereIsACorrectionFromTheStateBeforeIt()
        {
            _worldStep.SetReplaying(true);
            _simulation.Apply(3);
            _simulation.Step(9);

            _worldStep.SetReplaying(false);

            Assert.That(_corrections, Has.Count.EqualTo(1));
            Assert.That((_corrections[0].Before, _corrections[0].After), Is.EqualTo((5, 9)));
        }

        [Test]
        public void ReplayEndingWhereItStartedCorrectsNothing()
        {
            _worldStep.SetReplaying(true);
            _simulation.Apply(3);
            _simulation.Step(5);

            _worldStep.SetReplaying(false);

            Assert.That(_corrections, Is.Empty);
        }

        [Test]
        public void CorrectionWaitsForTheReplayToEnd()
        {
            _worldStep.SetReplaying(true);

            _simulation.Step(9);

            Assert.That(_corrections, Is.Empty);
        }

        [Test]
        public void ChangesOutsideAReplayAreNoCorrections()
        {
            _simulation.Step(9);
            _simulation.Apply(1);

            Assert.That(_corrections, Is.Empty);
        }

        [Test]
        public void EveryReplayStartsFromItsOwnState()
        {
            _worldStep.SetReplaying(true);
            _simulation.Step(9);
            _worldStep.SetReplaying(false);
            _simulation.Step(10);

            _worldStep.SetReplaying(true);
            _simulation.Step(12);
            _worldStep.SetReplaying(false);

            Assert.That(_corrections, Has.Count.EqualTo(2));
            Assert.That((_corrections[1].Before, _corrections[1].After), Is.EqualTo((10, 12)));
        }

        [Test]
        public void EndedCorrectionsStayQuiet()
        {
            var ended = new LifetimeDefinition();
            var corrections = new List<StateCorrection<int>>();
            new StateCorrections<int>(ended.Lifetime, _simulation, _worldStep).Corrected.Advise(_lifetime.Lifetime, corrections.Add);
            ended.Terminate();

            _worldStep.SetReplaying(true);
            _simulation.Step(9);
            _worldStep.SetReplaying(false);

            Assert.That(corrections, Is.Empty);
        }
    }
}
