using Bw.Entities.Extensions;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network.Prediction
{
    public sealed class ThrowConfirmation
    {
        private readonly IWeapon _prop;
        private readonly IReadonlySimulation<PropState> _simulation;
        private readonly IReplicatedStates<PropState> _states;
        private readonly INetworkTicks _ticks;
        private readonly SequentialLifetimes _waits;

        public ThrowConfirmation(
            Lifetime lifetime,
            IWeapon prop,
            IReadonlySimulation<PropState> simulation,
            IReplicatedStates<PropState> states,
            INetworkTicks ticks)
        {
            _prop = prop;
            _simulation = simulation;
            _states = states;
            _ticks = ticks;
            _waits = new SequentialLifetimes(lifetime);
        }

        public void Expect(ICarriedReplica replica)
        {
            var tick = _ticks.Current;
            var throws = _simulation.State.Value.Throws;
            _ticks.Ticked(TickPhase.Default).Advise(_waits.Next(), _ => Check(tick, throws, replica));
        }

        private void Check(int tick, int throws, ICarriedReplica replica)
        {
            if (!_states.TryGetAt(int.MaxValue, out var newest) || newest.Tick < tick)
                return;

            _waits.TerminateCurrent();
            if (!_states.TryGetAt(tick, out var thrown) || thrown.Tick < tick || thrown.State.Throws < throws)
                replica.Restore(_prop);
        }
    }
}
