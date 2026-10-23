using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Weapon.Extensions;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Spawning
{
    public sealed class CharacterArming
    {
        private readonly ICharacter _character;
        private readonly IHands _hands;
        private readonly StartingLoadout _loadout;

        public CharacterArming(
            Lifetime lifetime,
            ICharacter character,
            IHands hands,
            ICharacterRegistry registry,
            StartingLoadout loadout)
        {
            _character = character;
            _hands = hands;
            _loadout = loadout;

            registry.PlayerByCharacter.ViewForKey(lifetime, character, (registeredLifetime, _) =>
                character.Mortal.State.WhenAlive(registeredLifetime, Arm));
        }

        private void Arm(Lifetime aliveLifetime)
        {
            var position = (Vector3)_character.Movement.State.Value.Body.Position;
            var gun = _loadout.CreateGun(position);
            var fists = _loadout.CreateFists(aliveLifetime, position);
            _character.Inventory.Give(_character.Inventory.FreeSlotFor(fists), fists);
            _hands.Take(gun);
        }
    }
}
