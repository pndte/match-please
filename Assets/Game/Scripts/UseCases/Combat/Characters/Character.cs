using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement.Abstractions;

namespace Bw.UseCases.Combat.Characters
{
    public sealed class Character : ICharacter
    {
        public IReadonlyMortal Mortal { get; }
        public IReadonlyMovement Movement { get; }
        public IReadonlyHands Hands { get; }
        public IWeaponInventory Inventory { get; }
        IReadonlyWeaponInventory IReadonlyCharacter.Inventory => Inventory;

        public Character(
            IReadonlyMortal mortal,
            IReadonlyMovement movement,
            IReadonlyHands hands,
            IWeaponInventory inventory)
        {
            Mortal = mortal;
            Movement = movement;
            Hands = hands;
            Inventory = inventory;
        }
    }
}
