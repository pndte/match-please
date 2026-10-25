using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon
{
    public sealed class UnheldStateViewTests
    {
        private static readonly GunState From = new(0f, 3, 0, 0, 0);
        private static readonly GunState To = new(10f, 2, 4, 0, 1);

        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private WeaponCarriers _carriers;
        private RecordingView<GunState> _inner;
        private UnheldStateView<GunState> _view;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _carriers = new WeaponCarriers();
            _inner = new RecordingView<GunState>();
            _view = new UnheldStateView<GunState>(_inner, _weapon, _carriers);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponOnTheFloorIsShown()
        {
            _view.Show(From, To, 0.25f);

            Assert.That(_inner.Shown, Is.EqualTo(new[] { (From, To, 0.25f) }));
        }

        [Test]
        public void CarriedWeaponIsNotShown()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, new FakeCharacter());

            _view.Show(From, To, 0.25f);

            Assert.That(_inner.Shown, Is.Empty);
        }

        [Test]
        public void DroppedWeaponIsShownAgain()
        {
            var carried = new LifetimeDefinition();
            _carriers.ByWeapon.AddLifetimed(carried.Lifetime, _weapon, new FakeCharacter());
            carried.Terminate();

            _view.Show(From, To, 1f);

            Assert.That(_inner.Shown, Has.Count.EqualTo(1));
        }

        [Test]
        public void AnotherCarriedWeaponDoesNotHideThisOne()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, new FakeWeapon(), new FakeCharacter());

            _view.Show(From, To, 0f);

            Assert.That(_inner.Shown, Has.Count.EqualTo(1));
        }
    }
}
