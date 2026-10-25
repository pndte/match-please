using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class SimulationAffectableTests
    {
        [Test]
        public void EffectAppliesTheRulesToTheSimulatedState()
        {
            var state = new RestorableValue<int>(10);
            var affectable = new SimulationAffectable<int, int>(state, new Push());
            state.Simulate(12);

            affectable.Affect(StateCause.Of(new ActionId(1UL, 5)), 3);

            Assert.That(state.Applied, Is.EqualTo(new[] { 15 }));
            Assert.That(state.State.Value, Is.EqualTo(15));
        }

        [Test]
        public void CauseDoesNotChangeTheEffect()
        {
            var caused = new RestorableValue<int>(10);
            var uncaused = new RestorableValue<int>(10);

            new SimulationAffectable<int, int>(caused, new Push()).Affect(StateCause.Of(new ActionId(1UL, 5)), 3);
            new SimulationAffectable<int, int>(uncaused, new Push()).Affect(StateCause.None, 3);

            Assert.That(caused.Applied, Is.EqualTo(uncaused.Applied));
        }

        [Test]
        public void EveryEffectIsAppliedOnTopOfTheLastOne()
        {
            var state = new RestorableValue<int>(0);
            var affectable = new SimulationAffectable<int, int>(state, new Push());

            affectable.Affect(StateCause.None, 1);
            affectable.Affect(StateCause.None, 2);
            affectable.Affect(StateCause.None, 4);

            Assert.That(state.Applied, Is.EqualTo(new[] { 1, 3, 7 }));
        }

        private sealed class Push : IEffectRules<int, int>
        {
            public int Apply(int state, int effect) =>
                state + effect;
        }
    }
}
