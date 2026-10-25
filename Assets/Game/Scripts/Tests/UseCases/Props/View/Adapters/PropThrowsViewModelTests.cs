using System.Collections.Generic;
using Bw.UseCases.Props;
using Bw.UseCases.Props.View;
using Bw.UseCases.Props.View.Adapters;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Props.View.Adapters
{
    public sealed class PropThrowsViewModelTests
    {
        private LifetimeDefinition _lifetime;
        private PropThrowsViewModel _viewModel;
        private int _windUps;
        private List<PropThrow> _throws;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _viewModel = new PropThrowsViewModel();
            _windUps = 0;
            _throws = new List<PropThrow>();
            _viewModel.WindUpStarted.Advise(_lifetime.Lifetime, _ => _windUps++);
            _viewModel.Thrown.Advise(_lifetime.Lifetime, thrown => _throws.Add(thrown));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void FirstShownStateOnlySynchronizes()
        {
            Show(State(PropPhase.WindingUp, 4));

            Assert.That(_windUps, Is.Zero);
            Assert.That(_throws, Is.Empty);
        }

        [Test]
        public void EnteringTheWindUpIsAnnouncedOnce()
        {
            Show(State(PropPhase.Holding, 0));

            Show(State(PropPhase.WindingUp, 0));
            Show(State(PropPhase.WindingUp, 0));

            Assert.That(_windUps, Is.EqualTo(1));
        }

        [Test]
        public void EveryNewThrowIsAnnouncedAlongItsAim()
        {
            Show(State(PropPhase.Holding, 0));

            Show(State(PropPhase.Holding, 1, 90f));

            Assert.That(_throws, Has.Count.EqualTo(1));
            Assert.That(_throws[0].Direction.x, Is.EqualTo(0f).Within(1e-5f));
            Assert.That(_throws[0].Direction.y, Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void SeveralThrowsAtOnceAreAnnouncedOnce()
        {
            Show(State(PropPhase.Holding, 0));

            Show(State(PropPhase.Holding, 3));

            Assert.That(_throws, Has.Count.EqualTo(1));
        }

        [Test]
        public void FewerThrowsAnnounceNothing()
        {
            Show(State(PropPhase.Holding, 2));

            Show(State(PropPhase.Holding, 1));
            Show(State(PropPhase.Holding, 1));

            Assert.That(_throws, Is.Empty);
        }

        [Test]
        public void SameStateShownAgainAnnouncesNothing()
        {
            Show(State(PropPhase.Holding, 0));
            Show(State(PropPhase.WindingUp, 0));
            Show(State(PropPhase.Holding, 1));

            Show(State(PropPhase.Holding, 1));

            Assert.That(_windUps, Is.EqualTo(1));
            Assert.That(_throws, Has.Count.EqualTo(1));
        }

        [Test]
        public void OnlyTheShownTargetStateCounts()
        {
            Show(State(PropPhase.Holding, 0));

            _viewModel.Show(State(PropPhase.WindingUp, 5), State(PropPhase.Holding, 0), 0.5f);

            Assert.That(_windUps, Is.Zero);
            Assert.That(_throws, Is.Empty);
        }

        private void Show(PropState state) =>
            _viewModel.Show(state, state, 1f);

        private static PropState State(PropPhase phase, int throws, float aim = 0f) =>
            new(aim, phase, phase == PropPhase.WindingUp ? 1 : 0, throws, false);
    }
}
