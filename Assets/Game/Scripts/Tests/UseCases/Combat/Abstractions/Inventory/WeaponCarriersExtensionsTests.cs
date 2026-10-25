using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory.Extensions;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Abstractions.Inventory
{
    public sealed class WeaponCarriersExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private WeaponCarriers _carriers;
        private FakeWeapon _weapon;
        private Bw.UseCases.Combat.Characters.Character _carrier;
        private List<Lifetime> _uncarried;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _carriers = new WeaponCarriers();
            _weapon = new FakeWeapon();
            _carrier = new Bw.UseCases.Combat.Characters.Character(new FakeMortal(), null, null, null);
            _uncarried = new List<Lifetime>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponNobodyCarriesIsUncarriedAtOnce()
        {
            _carriers.ViewUncarried(_lifetime.Lifetime, _weapon, _uncarried.Add);

            Assert.That(_uncarried, Has.Count.EqualTo(1));
            Assert.That(_uncarried[0].IsAlive, Is.True);
        }

        [Test]
        public void CarryEndsTheUncarriedLifetime()
        {
            _carriers.ViewUncarried(_lifetime.Lifetime, _weapon, _uncarried.Add);

            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);

            Assert.That(_uncarried, Has.Count.EqualTo(1));
            Assert.That(_uncarried[0].IsAlive, Is.False);
        }

        [Test]
        public void EndOfTheCarryStartsANewUncarriedLifetime()
        {
            var carry = new LifetimeDefinition(_lifetime.Lifetime);
            _carriers.ViewUncarried(_lifetime.Lifetime, _weapon, _uncarried.Add);
            _carriers.ByWeapon.AddLifetimed(carry.Lifetime, _weapon, _carrier);

            carry.Terminate();

            Assert.That(_uncarried, Has.Count.EqualTo(2));
            Assert.That(_uncarried[1].IsAlive, Is.True);
        }

        [Test]
        public void WeaponCarriedFromTheStartIsUncarriedOnlyAfterItsCarry()
        {
            var carry = new LifetimeDefinition(_lifetime.Lifetime);
            _carriers.ByWeapon.AddLifetimed(carry.Lifetime, _weapon, _carrier);
            _carriers.ViewUncarried(_lifetime.Lifetime, _weapon, _uncarried.Add);
            var beforeTheEnd = _uncarried.Count;

            carry.Terminate();

            Assert.That(beforeTheEnd, Is.Zero);
            Assert.That(_uncarried, Has.Count.EqualTo(1));
        }

        [Test]
        public void CarryOfAnotherWeaponChangesNothing()
        {
            _carriers.ViewUncarried(_lifetime.Lifetime, _weapon, _uncarried.Add);

            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, new FakeWeapon(), _carrier);

            Assert.That(_uncarried, Has.Count.EqualTo(1));
            Assert.That(_uncarried[0].IsAlive, Is.True);
        }

        [Test]
        public void EndOfTheViewEndsTheUncarriedLifetime()
        {
            var view = new LifetimeDefinition(_lifetime.Lifetime);
            _carriers.ViewUncarried(view.Lifetime, _weapon, _uncarried.Add);

            view.Terminate();

            Assert.That(_uncarried[0].IsAlive, Is.False);
        }

        [Test]
        public void CarryEndingAfterTheViewStartsNothing()
        {
            var view = new LifetimeDefinition(_lifetime.Lifetime);
            var carry = new LifetimeDefinition(_lifetime.Lifetime);
            _carriers.ByWeapon.AddLifetimed(carry.Lifetime, _weapon, _carrier);
            _carriers.ViewUncarried(view.Lifetime, _weapon, _uncarried.Add);
            view.Terminate();

            carry.Terminate();

            Assert.That(_uncarried, Is.Empty);
        }
    }
}
