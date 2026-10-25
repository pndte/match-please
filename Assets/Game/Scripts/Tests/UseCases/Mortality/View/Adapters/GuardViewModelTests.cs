using System.Collections.Generic;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.View.Adapters;
using Bw.UseCases.Mortality.View.Health;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.View.Adapters
{
    public sealed class GuardViewModelTests
    {
        private LifetimeDefinition _lifetime;
        private ShownGuard _guard;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _guard = new ShownGuard(new GuardState(50f, false, false));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void NewViewModelShowsTheShareOfTheGuardWithoutChange()
        {
            var viewModel = new GuardViewModel(_lifetime.Lifetime, _guard);

            Assert.That(viewModel.Max, Is.EqualTo(200f));
            Assert.That(viewModel.Level.Value, Is.EqualTo(new HealthLevel(0.25f, 0f)));
            Assert.That(viewModel.Up.Value, Is.False);
        }

        [Test]
        public void BlowShowsItsLossAsTheChange()
        {
            var viewModel = new GuardViewModel(_lifetime.Lifetime, _guard);

            _guard.Show(new GuardState(30f, true, false));

            Assert.That(viewModel.Level.Value, Is.EqualTo(new HealthLevel(0.15f, -0.1f)));
        }

        [Test]
        public void EveryChangeIsMeasuredFromTheLastShownHealth()
        {
            var viewModel = new GuardViewModel(_lifetime.Lifetime, _guard);
            var levels = new List<HealthLevel>();
            viewModel.Level.Advise(_lifetime.Lifetime, levels.Add);

            _guard.Show(new GuardState(30f, true, false));
            _guard.Show(new GuardState(70f, true, false));

            Assert.That(levels, Is.EqualTo(new[] { new HealthLevel(0.25f, 0f), new HealthLevel(0.15f, -0.1f), new HealthLevel(0.35f, 0.2f) }));
        }

        [Test]
        public void StanceChangeWithoutBlowShowsNoChange()
        {
            var viewModel = new GuardViewModel(_lifetime.Lifetime, _guard);
            _guard.Show(new GuardState(30f, false, false));

            _guard.Show(new GuardState(30f, true, false));

            Assert.That(viewModel.Level.Value, Is.EqualTo(new HealthLevel(0.15f, 0f)));
        }

        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        public void UpFollowsTheGuard(bool raised, bool broken, bool up)
        {
            var viewModel = new GuardViewModel(_lifetime.Lifetime, _guard);

            _guard.Show(new GuardState(10f, raised, broken));

            Assert.That(viewModel.Up.Value, Is.EqualTo(up));
        }

        private sealed class ShownGuard : IReadonlyGuard
        {
            public float Max => 200f;
            public IReadonlyProperty<GuardState> State => _state;

            private readonly ViewableProperty<GuardState> _state;

            public ShownGuard(GuardState initial)
            {
                _state = new ViewableProperty<GuardState>(initial);
            }

            public void Show(GuardState state) =>
                _state.Value = state;
        }
    }
}
