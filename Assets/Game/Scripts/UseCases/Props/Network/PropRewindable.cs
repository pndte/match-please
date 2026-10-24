using Bw.Entities.Extensions;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropRewindable : IRewindable
    {
        private readonly IRewindable _body;
        private readonly SimulationRewinder<PropState> _aim;
        private readonly IWeapon _prop;
        private readonly IWeaponCarriers _carriers;

        private int _releaseTick = int.MinValue;

        public PropRewindable(
            Lifetime lifetime,
            IRewindable body,
            SimulationRewinder<PropState> aim,
            IWeapon prop,
            IWeaponCarriers carriers,
            INetworkTicks ticks)
        {
            _body = body;
            _aim = aim;
            _prop = prop;
            _carriers = carriers;

            carriers.ByWeapon.ViewForKey(lifetime, prop, (carriedLifetime, _) =>
                carriedLifetime.OnTermination(() => _releaseTick = ticks.Current));
        }

        public void Rewind(Lifetime lifetime, ShooterView view)
        {
            if (_carriers.ByWeapon.ContainsKey(_prop)) //TODO: ящик, который стрелок видел ещё на полу, проверяется в руках откаченного держателя: откат держателя ставит в руки тело хвата, а место на полу не откатывается
                _aim.Rewind(lifetime, view);
            else if (view.Interpolated >= _releaseTick) //TODO: сервер не помнит, где ящик был в руках: если стрелок видел ящик в руках, а держатель уже отпустил его, выстрел проверяется по месту, где ящик сейчас
                _body.Rewind(lifetime, view);
        }
    }
}
