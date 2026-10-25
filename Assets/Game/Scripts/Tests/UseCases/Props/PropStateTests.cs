using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Props;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Props
{
    public sealed class PropStateTests
    {
        [Test]
        public void WeaponViewReadsTheAimAndCountsThrowsAsShots()
        {
            IWeaponState state = new PropState(42f, PropPhase.Holding, 0, 3, false);

            Assert.That(state.Aim, Is.EqualTo(42f));
            Assert.That(state.Shots, Is.EqualTo(3));
        }

        [TestCase(PropPhase.Holding, true)]
        [TestCase(PropPhase.WindingUp, false)]
        public void PropIsStowableOnlyWhileHolding(PropPhase phase, bool stowable)
        {
            IWeaponState state = new PropState(0f, phase, 1, 0, false);

            Assert.That(state.Stowable, Is.EqualTo(stowable));
        }

        [TestCase(PropPhase.Holding)]
        [TestCase(PropPhase.WindingUp)]
        public void PropNeverRootsItsHolder(PropPhase phase)
        {
            IWeaponState state = new PropState(0f, phase, 1, 0, true);

            Assert.That(state.Rooting, Is.False);
        }

        [Test]
        public void StatesWithTheSameFieldsAreEqual()
        {
            var state = new PropState(10f, PropPhase.WindingUp, 2, 1, true);

            Assert.That(state.Equals(new PropState(10f, PropPhase.WindingUp, 2, 1, true)), Is.True);
        }

        [Test]
        public void EveryFieldTakesPartInEquality()
        {
            var state = new PropState(10f, PropPhase.WindingUp, 2, 1, true);

            Assert.That(state.Equals(new PropState(11f, PropPhase.WindingUp, 2, 1, true)), Is.False);
            Assert.That(state.Equals(new PropState(10f, PropPhase.Holding, 2, 1, true)), Is.False);
            Assert.That(state.Equals(new PropState(10f, PropPhase.WindingUp, 3, 1, true)), Is.False);
            Assert.That(state.Equals(new PropState(10f, PropPhase.WindingUp, 2, 2, true)), Is.False);
            Assert.That(state.Equals(new PropState(10f, PropPhase.WindingUp, 2, 1, false)), Is.False);
        }
    }
}
