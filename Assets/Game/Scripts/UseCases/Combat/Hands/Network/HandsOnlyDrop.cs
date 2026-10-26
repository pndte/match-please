using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands.Extensions;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class HandsOnlyDrop
    {
        private readonly ICharacter _character;

        public HandsOnlyDrop(Lifetime lifetime, ICharacter character)
        {
            _character = character;

            character.Mortal.State.WhenAlive(lifetime, aliveLifetime =>
                character.Hands.State.Select(aliveLifetime, state => state.InHands).View(aliveLifetime, (inHandsLifetime, slot) =>
                    inHandsLifetime.OnTermination(() =>
                    {
                        if (aliveLifetime.IsAlive)
                            Drop(slot);
                    })));
        }

        private void Drop(HandsSlot left)
        {
            var inventory = _character.Inventory;
            if (inventory.TryFindTop(left, out var top) && inventory.RulesOf(top).HandsOnly)
                inventory.Drop(top);
        }
    }
}
