using Bw.Entities.Extensions;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsPublisher
    {
        private readonly CarriedWeaponsReport _report;
        private readonly INetResultSender<CarriedWeapons> _carried;

        private bool _changed;

        public CarriedWeaponsPublisher(
            Lifetime lifetime,
            IReadonlyWeaponInventory inventory,
            CarriedWeaponsReport report,
            INetResultSender<CarriedWeapons> carried,
            INetworkTicks ticks)
        {
            _report = report;
            _carried = carried;

            inventory.Carried.View(lifetime, (carriedLifetime, _, _) => Change(carriedLifetime));
            ticks.Ticked(TickPhase.Late).Advise(lifetime, _ => Publish());
        }

        private void Change(Lifetime carryLifetime)
        {
            _changed = true;
            carryLifetime.OnTermination(() => _changed = true);
        }

        private void Publish()
        {
            if (!_changed)
                return;

            _changed = false;
            _carried.Broadcast(_report.Current);
        }
    }
}
