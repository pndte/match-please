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
        private readonly IReadonlyWeaponCarriers _carriers;

        private int _releaseTick = int.MinValue;

        public PropRewindable(
            Lifetime lifetime,
            IRewindable body,
            SimulationRewinder<PropState> aim,
            IWeapon prop,
            IReadonlyWeaponCarriers carriers,
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
            if (_carriers.ByWeapon.ContainsKey(_prop))
                _aim.Rewind(lifetime, view);
            else if (view.Interpolated >= _releaseTick)
                _body.Rewind(lifetime, view);
        }
    }
}
