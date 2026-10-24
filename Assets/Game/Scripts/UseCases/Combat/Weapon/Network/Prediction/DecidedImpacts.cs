using System;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network.Prediction
{
    public sealed class DecidedImpacts
    {
        private readonly TickHistory<Strike> _decided;
        private readonly IAffectables<IReadonlyMortal, Impacts> _predictedBodies;

        public DecidedImpacts(
            Lifetime lifetime,
            IStrikes strikes,
            IWorldStep worldStep,
            IAffectables<IReadonlyMortal, Impacts> predictedBodies,
            WorldRollbackConfig config)
        {
            _predictedBodies = predictedBodies;
            _decided = new TickHistory<Strike>(config.HistoryTicks);

            strikes.Struck.Advise(lifetime, Decide);
            worldStep.Replaying.WhenTrue(lifetime, replayLifetime =>
                worldStep.Stepped(TickPhase.Late).Advise(replayLifetime, Repeat));
        }

        private void Decide(Strike strike)
        {
            if (_decided.TryGetAt(strike.Tick, out _))
                throw new InvalidOperationException($"Impacts are decided once a tick, and tick {strike.Tick.ToString()} already has its decision.");

            _decided.Record(strike.Tick, strike);
            Knock(strike);
        }

        private void Repeat(int tick)
        {
            if (_decided.TryGetAt(tick, out var strike))
                Knock(strike);
        }

        private void Knock(Strike strike)
        {
            for (var index = 0; index < strike.Affected.Count; index++)
            {
                var affected = strike.Affected[index];
                if (_predictedBodies.TryGet(affected.Target, out _, out var body))
                    body.Affect(StateCause.None, affected.Effect);
            }
        }
    }
}
