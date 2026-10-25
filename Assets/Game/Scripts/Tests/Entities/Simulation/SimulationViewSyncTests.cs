using System.Collections.Generic;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Simulation
{
    public sealed class SimulationViewSyncTests
    {
        private LifetimeDefinition _lifetime;
        private Simulation _simulation;
        private RecordingView _view;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _simulation = new Simulation(7);
            _view = new RecordingView();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void CurrentStateIsShownRightAway()
        {
            new SimulationViewSync<int>(_lifetime.Lifetime, _simulation, _view);

            Assert.That(_view.Shown, Is.EqualTo(new[] { (7, 7, 0f) }));
        }

        [Test]
        public void EveryNewStateIsShownAtRest()
        {
            new SimulationViewSync<int>(_lifetime.Lifetime, _simulation, _view);

            _simulation.Become(8);
            _simulation.Become(9);

            Assert.That(_view.Shown, Is.EqualTo(new[] { (7, 7, 0f), (8, 8, 0f), (9, 9, 0f) }));
        }

        [Test]
        public void SyncStopsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            new SimulationViewSync<int>(lifetime.Lifetime, _simulation, _view);
            lifetime.Terminate();

            _simulation.Become(8);

            Assert.That(_view.Shown, Is.EqualTo(new[] { (7, 7, 0f) }));
        }

        private sealed class Simulation : IReadonlySimulation<int>
        {
            public IReadonlyProperty<int> State => _state;
            public int Previous { get; private set; }
            public ISource<int> Stepped => _stepped;

            private readonly ViewableProperty<int> _state;
            private readonly Signal<int> _stepped = new();

            public Simulation(int initial)
            {
                _state = new ViewableProperty<int>(initial);
            }

            public void Become(int state)
            {
                Previous = _state.Value;
                _state.Value = state;
                _stepped.Fire(state);
            }
        }

        private sealed class RecordingView : IStateView<int>
        {
            public List<(int, int, float)> Shown { get; } = new();

            public void Show(int from, int to, float progress) =>
                Shown.Add((from, to, progress));
        }
    }
}
