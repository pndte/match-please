using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon.Abstractions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Shooting
{
    public sealed class GunStateTests
    {
        private static readonly GunState Sample = new(30f, 4, 2, 1, 9);

        private static readonly GunState[] Others =
        {
            new(31f, 4, 2, 1, 9),
            new(30f, 5, 2, 1, 9),
            new(30f, 4, 3, 1, 9),
            new(30f, 4, 2, 0, 9),
            new(30f, 4, 2, 1, 8)
        };

        [Test]
        public void StatesWithTheSameFieldsAreEqual()
        {
            Assert.That(Sample.Equals(new GunState(30f, 4, 2, 1, 9)), Is.True);
        }

        [TestCaseSource(nameof(Others))]
        public void StatesDifferingInAnyFieldDiffer(GunState other)
        {
            Assert.That(Sample.Equals(other), Is.False);
        }

        [Test]
        public void GunIsSeenAsAWeaponStateByItsAimAndShots()
        {
            IWeaponState state = Sample;

            Assert.That(state.Aim, Is.EqualTo(30f));
            Assert.That(state.Shots, Is.EqualTo(9));
        }

        [Test]
        public void GunCanAlwaysBeStowedAndNeverRootsItsHolder()
        {
            IWeaponState state = Sample;

            Assert.That(state.Stowable, Is.True);
            Assert.That(state.Rooting, Is.False);
        }

        [Test]
        public void TurningChangesOnlyTheAim()
        {
            IAimedState<GunState> state = Sample;

            Assert.That(state.Turned(-12.5f), Is.EqualTo(new GunState(17.5f, 4, 2, 1, 9)));
        }
    }
}
