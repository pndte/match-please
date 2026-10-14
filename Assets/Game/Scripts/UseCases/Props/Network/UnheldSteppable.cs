using Bw.Entities.Extensions;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class UnheldSteppable : ISteppable
    {
        private readonly ISteppable _steppable;
        private readonly IWeaponHold _hold;
        private readonly INetworkTicks _ticks;

        private int _releaseTick = int.MinValue;

        public UnheldSteppable(
            Lifetime lifetime,
            ISteppable steppable,
            IWeaponHold hold,
            INetworkTicks ticks)
        {
            _steppable = steppable;
            _hold = hold;
            _ticks = ticks;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
                heldLifetime.OnTermination(() => _releaseTick = ticks.Current));
        }

        public void Step()
        {
            if (_hold.HeldLifetime.Value.IsAlive || _ticks.Current == _releaseTick)
                return;

            _steppable.Step();
        }
    }
}
