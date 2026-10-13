using Bw.Entities.Extensions;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropRewindable : IRewindable
    {
        private readonly IRewindable _body;
        private readonly SimulationRewinder<PropState> _aim;
        private readonly IWeaponHold _hold;

        private int _releaseTick = int.MinValue;

        public PropRewindable(
            Lifetime lifetime,
            IRewindable body,
            SimulationRewinder<PropState> aim,
            IWeaponHold hold,
            INetworkTicks ticks)
        {
            _body = body;
            _aim = aim;
            _hold = hold;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
                heldLifetime.OnTermination(() => _releaseTick = ticks.Current));
        }

        public void Rewind(Lifetime lifetime, SeenTicks seen)
        {
            if (_hold.HeldLifetime.Value.IsAlive) //TODO: ящик, который стрелок видел ещё на полу, проверяется в руках откаченного держателя: откат держателя снова кладёт ящик в руки (HoldingBody) и затирает место на полу
                _aim.Rewind(lifetime, seen);
            else if (seen.Interpolated >= _releaseTick) //TODO: сервер не помнит, где ящик был в руках: если стрелок видел ящик в руках, а держатель уже отпустил его, выстрел проверяется по месту, где ящик сейчас
                _body.Rewind(lifetime, seen);
        }
    }
}
