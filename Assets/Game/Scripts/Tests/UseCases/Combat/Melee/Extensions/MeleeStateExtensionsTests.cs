using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Melee.Extensions;
using NUnit.Framework;
using static Bw.Tests.UseCases.Combat.Melee.Doubles.EighthSecondFists;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Melee.Extensions
{
    public sealed class MeleeStateExtensionsTests
    {
        private MeleeWeaponConfig _config;
        private ManualClock _step;

        [SetUp]
        public void SetUp()
        {
            _config = Create();
            _step = new ManualClock(Step);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(MeleePhase.Charging, FullChargeTicks - 1, false)]
        [TestCase(MeleePhase.Charging, FullChargeTicks, true)]
        [TestCase(MeleePhase.Charging, FullChargeTicks + FullChargeHoldTicks, true)]
        [TestCase(MeleePhase.StrongSwing, FullChargeTicks, false)]
        [TestCase(MeleePhase.Idle, FullChargeTicks, false)]
        public void FistsAreFullyChargedOnlyWhileChargingAtTheFullCharge(MeleePhase phase, int chargeTicks, bool full)
        {
            var state = new MeleeState(0f, phase, chargeTicks, false, chargeTicks, 0);

            Assert.That(state.FullyCharged(_config, _step), Is.EqualTo(full));
        }

        [TestCase(MeleePhase.StrongSwing, 0, true)]
        [TestCase(MeleePhase.StrongSwing, 1, false)]
        [TestCase(MeleePhase.Jab, 0, false)]
        [TestCase(MeleePhase.Recovery, 0, false)]
        public void StrongSwingStartsOnlyOnItsFirstTick(MeleePhase phase, int phaseTicks, bool started)
        {
            var state = new MeleeState(0f, phase, phaseTicks, false, 0, 0);

            Assert.That(state.StrongSwingStarted(), Is.EqualTo(started));
        }

        [TestCase(MeleePhase.Jab, 0, true)]
        [TestCase(MeleePhase.Jab, StrikeTicks - 1, true)]
        [TestCase(MeleePhase.Jab, StrikeTicks, false)]
        [TestCase(MeleePhase.StrongSwing, StrongStrikeTicks - 1, true)]
        [TestCase(MeleePhase.StrongSwing, StrongStrikeTicks, false)]
        [TestCase(MeleePhase.Idle, 0, false)]
        [TestCase(MeleePhase.Charging, 0, false)]
        [TestCase(MeleePhase.Recovery, 0, false)]
        public void StrikeIsPendingOnlyBeforeTheSwingsStrikeTick(MeleePhase phase, int phaseTicks, bool pending)
        {
            var state = new MeleeState(0f, phase, phaseTicks, false, 0, 0);

            Assert.That(state.StrikePending(_config, _step), Is.EqualTo(pending));
        }
    }
}
