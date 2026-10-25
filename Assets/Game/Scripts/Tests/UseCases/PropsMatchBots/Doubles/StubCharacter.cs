using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubCharacter : ICharacter
    {
        public StubMortal Mortal { get; } = new();
        public StubMovement Movement { get; }
        public StubHands Hands { get; } = new();
        public StubInventory Inventory { get; }

        IReadonlyMortal IReadonlyCharacter.Mortal => Mortal;
        IReadonlyMovement IReadonlyCharacter.Movement => Movement;
        IReadonlyHands IReadonlyCharacter.Hands => Hands;
        IReadonlyWeaponInventory IReadonlyCharacter.Inventory => Inventory;
        IWeaponInventory ICharacter.Inventory => Inventory;

        public StubCharacter(Vector2 position = default, params WeaponSlot[] slots)
        {
            Movement = new StubMovement(position);
            Inventory = new StubInventory(slots);
        }
    }
}
