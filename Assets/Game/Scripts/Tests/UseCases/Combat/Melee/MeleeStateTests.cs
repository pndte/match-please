using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Weapon.Abstractions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Melee
{
    public sealed class MeleeStateTests
    {
        [TestCase(MeleePhase.Idle, false, true)]
        [TestCase(MeleePhase.Jab, false, true)]
        [TestCase(MeleePhase.Jab, true, false)]
        [TestCase(MeleePhase.Charging, false, false)]
        [TestCase(MeleePhase.StrongSwing, false, false)]
        [TestCase(MeleePhase.Recovery, false, false)]
        public void FistsAreStowableOnlyWhenIdleOrInAReleasedJab(MeleePhase phase, bool held, bool stowable)
        {
            IWeaponState state = new MeleeState(0f, phase, 1, held, 0, 0);

            Assert.That(state.Stowable, Is.EqualTo(stowable));
        }

        [TestCase(MeleePhase.Idle, false)]
        [TestCase(MeleePhase.Jab, false)]
        [TestCase(MeleePhase.Charging, false)]
        [TestCase(MeleePhase.StrongSwing, true)]
        [TestCase(MeleePhase.Recovery, true)]
        public void FistsRootThroughTheStrongSwingAndTheRecovery(MeleePhase phase, bool rooting)
        {
            IWeaponState state = new MeleeState(0f, phase, 1, false, 0, 0);

            Assert.That(state.Rooting, Is.EqualTo(rooting));
        }

        [Test]
        public void WeaponStateShowsTheAimAndTheStrikes()
        {
            IWeaponState state = new MeleeState(42f, MeleePhase.Jab, 1, true, 0, 9);

            Assert.That(state.Aim, Is.EqualTo(42f));
            Assert.That(state.Shots, Is.EqualTo(9));
        }

        [Test]
        public void TurnedStateKeepsEverythingButTheAim()
        {
            IAimedState<MeleeState> state = new MeleeState(10f, MeleePhase.Charging, 3, false, 3, 2);

            var turned = state.Turned(25f);

            Assert.That(turned, Is.EqualTo(new MeleeState(35f, MeleePhase.Charging, 3, false, 3, 2)));
        }

        [Test]
        public void StatesOfEqualFieldsAreEqual()
        {
            Assert.That(new MeleeState(1f, MeleePhase.Jab, 2, true, 3, 4).Equals(new MeleeState(1f, MeleePhase.Jab, 2, true, 3, 4)), Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void StatesDifferingInOneFieldDiffer(int field)
        {
            var state = new MeleeState(1f, MeleePhase.Jab, 2, true, 3, 4);
            var other = field switch
            {
                0 => new MeleeState(1.5f, MeleePhase.Jab, 2, true, 3, 4),
                1 => new MeleeState(1f, MeleePhase.Idle, 2, true, 3, 4),
                2 => new MeleeState(1f, MeleePhase.Jab, 3, true, 3, 4),
                3 => new MeleeState(1f, MeleePhase.Jab, 2, false, 3, 4),
                4 => new MeleeState(1f, MeleePhase.Jab, 2, true, 4, 4),
                _ => new MeleeState(1f, MeleePhase.Jab, 2, true, 3, 5)
            };

            Assert.That(state.Equals(other), Is.False);
        }
    }
}
