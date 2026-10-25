using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement.Abstractions;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeCharacter : ICharacter
    {
        public FakeMortal Mortal { get; }
        public FakeMovement Movement { get; } = new();
        public FakeHands Hands { get; } = new();
        public IWeaponInventory Inventory { get; }

        IReadonlyMortal IReadonlyCharacter.Mortal => Mortal;
        IReadonlyMovement IReadonlyCharacter.Movement => Movement;
        IReadonlyHands IReadonlyCharacter.Hands => Hands;
        IReadonlyWeaponInventory IReadonlyCharacter.Inventory => Inventory;

        public FakeCharacter() : this(new FakeMortal(), new RecordingInventory())
        {
        }

        public FakeCharacter(FakeMortal mortal) : this(mortal, new RecordingInventory())
        {
        }

        public FakeCharacter(FakeMortal mortal, IWeaponInventory inventory)
        {
            Mortal = mortal;
            Inventory = inventory;
        }
    }
}
