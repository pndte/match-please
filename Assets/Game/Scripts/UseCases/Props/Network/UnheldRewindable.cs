using Bw.Entities.Extensions;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class UnheldRewindable : IRewindable
    {
        private readonly IRewindable _rewindable;
        private readonly IWeaponHold _hold;

        private int _releaseTick = int.MinValue;

        public UnheldRewindable(Lifetime lifetime, IRewindable rewindable, IWeaponHold hold, INetworkTicks ticks)
        {
            _rewindable = rewindable;
            _hold = hold;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
                heldLifetime.OnTermination(() => _releaseTick = ticks.Current));
        }

        public void Rewind(Lifetime lifetime, SeenTicks seen)
        {
            if (_hold.HeldLifetime.Value.IsAlive || seen.Interpolated < _releaseTick) //TODO: сервер не помнит, где ящик был в руках: если стрелок видел ящик в руках, а держатель уже отпустил его, выстрел проверяется по месту, где ящик сейчас
                return;

            _rewindable.Rewind(lifetime, seen);
        }
    }
}
