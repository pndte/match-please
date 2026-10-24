using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsPublisher
    {
        private readonly CarriedWeaponsReport _report;
        private readonly INetResultSender<TickedState<CarriedWeapons>> _carried;
        private readonly WorldRollbackConfig _config;

        private bool _changed = true;
        private int _sentTick;

        public CarriedWeaponsPublisher(
            Lifetime lifetime,
            IReadonlyWeaponInventory inventory,
            CarriedWeaponsReport report,
            INetResultSender<TickedState<CarriedWeapons>> carried,
            IWorldStep worldStep,
            WorldRollbackConfig config)
        {
            _report = report;
            _carried = carried;
            _config = config;

            inventory.Carried.Advise(lifetime, _ => _changed = true);
            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, Publish);
        }

        private void Publish(int tick)
        {
            if (!_changed && tick - _sentTick < _config.UnchangedStateTicks)
                return;

            _changed = false;
            _sentTick = tick;
            _carried.Broadcast(new TickedState<CarriedWeapons>(tick, _report.Current));
        }
    }
}
