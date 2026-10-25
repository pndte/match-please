using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Hands.Extensions;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class BusyHandsGuard : ISimulator<HandsInput, HandsState>
    {
        private readonly ISimulator<HandsInput, HandsState> _simulator;
        private readonly IReadonlyWeaponInventory _inventory;

        public BusyHandsGuard(ISimulator<HandsInput, HandsState> simulator, IReadonlyWeaponInventory inventory)
        {
            _simulator = simulator;
            _inventory = inventory;
        }

        public HandsState Step(HandsState state, HandsInput input) =>
            _simulator.Step(state, state.Busy(_inventory) ? new HandsInput(input.Weapon, DrawChoice.Keep) : input);
    }
}
