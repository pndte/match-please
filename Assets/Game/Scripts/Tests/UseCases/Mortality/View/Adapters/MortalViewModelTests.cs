using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.View.Adapters;
using Bw.UseCases.Mortality.View.Health;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.View.Adapters
{
    public sealed class MortalViewModelTests
    {
        private LifetimeDefinition _lifetime;
        private MortalFixture _fixture;
        private MortalViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _fixture = new MortalFixture(80f);
            _viewModel = new MortalViewModel(_lifetime.Lifetime, _fixture.Mortal);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _fixture.Dispose();
        }

        [Test]
        public void NewMortalShowsAFullBarOfItsMaximum()
        {
            Assert.That(_viewModel.Max, Is.EqualTo(80f));
            Assert.That(_viewModel.Level.Value, Is.EqualTo(HealthLevel.Full));
            Assert.That(_viewModel.Dead.Value, Is.False);
        }

        [Test]
        public void LevelIsTheShareOfHealthAndOfItsChange()
        {
            _fixture.Mortal.Apply(VitalsOf.Alive(60f, -20f));

            Assert.That(_viewModel.Level.Value, Is.EqualTo(new HealthLevel(0.75f, -0.25f)));
        }

        [Test]
        public void HealShowsAPositiveChange()
        {
            _fixture.Mortal.Apply(VitalsOf.Alive(40f, -40f));

            _fixture.Mortal.Apply(VitalsOf.Alive(60f, 20f));

            Assert.That(_viewModel.Level.Value, Is.EqualTo(new HealthLevel(0.75f, 0.25f)));
        }

        [Test]
        public void DamageIsTaken()
        {
            var taken = 0;
            _viewModel.Taken.Advise(_lifetime.Lifetime, _ => taken++);

            _fixture.Mortal.Apply(VitalsOf.Alive(60f, -20f));

            Assert.That(taken, Is.EqualTo(1));
        }

        [Test]
        public void HealsAndSnapshotsAreNotTaken()
        {
            var taken = 0;
            _viewModel.Taken.Advise(_lifetime.Lifetime, _ => taken++);

            _fixture.Mortal.Apply(VitalsOf.Alive(60f));
            _fixture.Mortal.Apply(VitalsOf.Alive(70f, 10f));

            Assert.That(taken, Is.Zero);
        }

        [Test]
        public void DeathShowsDeadAndAKill()
        {
            var killed = 0;
            _viewModel.Killed.Advise(_lifetime.Lifetime, _ => killed++);

            _fixture.Mortal.Apply(_fixture.Mortal.Current.AfterHit(100f));

            Assert.That(_viewModel.Dead.Value, Is.True);
            Assert.That(killed, Is.EqualTo(1));
        }

        [Test]
        public void LateJoinDeathShowsDeadWithoutAKill()
        {
            var killed = 0;
            _viewModel.Killed.Advise(_lifetime.Lifetime, _ => killed++);

            _fixture.Mortal.Apply(VitalsOf.Dead());

            Assert.That(_viewModel.Dead.Value, Is.True);
            Assert.That(killed, Is.Zero);
        }
    }
}
