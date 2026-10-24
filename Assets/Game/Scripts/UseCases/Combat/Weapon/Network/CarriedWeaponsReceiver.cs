using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsReceiver : TickHistory<InventoryContent>, IReplicatedStates<InventoryContent>
    {
        private readonly IReadonlyCharacter _character;
        private readonly INetworkWeapons _weapons;

        private TickedState<CarriedWeapons> _waiting;
        private bool _unresolved;

        public CarriedWeaponsReceiver(
            Lifetime lifetime,
            IReadonlyCharacter character,
            INetResultReceiver<TickedState<CarriedWeapons>> carried,
            INetworkWeapons weapons,
            WorldRollbackConfig config)
            : base(config.HistoryTicks)
        {
            _character = character;
            _weapons = weapons;

            character.Mortal.State.WhenAlive(lifetime, aliveLifetime =>
            {
                carried.Received.Advise(aliveLifetime, Receive);
                weapons.ByIds.Advise(aliveLifetime, _ => RecordWaiting());
                aliveLifetime.OnTermination(Forget);
            });
        }

        private void Receive(TickedState<CarriedWeapons> carried)
        {
            _waiting = carried;
            _unresolved = true;
            RecordWaiting();
        }

        private void RecordWaiting()
        {
            if (!_unresolved || !TryResolve(_waiting.State, out var content))
                return;

            _unresolved = false;
            if (NewestTick == int.MinValue || _waiting.Tick > NewestTick - Capacity)
                Record(_waiting.Tick, content);
        }

        private bool TryResolve(CarriedWeapons carried, out InventoryContent content)
        {
            var items = new InventoryItem[carried.Weapons.Length];
            for (var index = 0; index < items.Length; index++)
            {
                if (!_weapons.ByIds.TryGetValue(carried.Weapons[index].Weapon, out var weapon))
                {
                    content = default;
                    return false;
                }

                items[index] = new InventoryItem(weapon, _character.Inventory.Slots[carried.Weapons[index].Slot]);
            }

            content = new InventoryContent(items);
            return true;
        }

        private void Forget()
        {
            Clear();
            _unresolved = false;
        }
    }
}
