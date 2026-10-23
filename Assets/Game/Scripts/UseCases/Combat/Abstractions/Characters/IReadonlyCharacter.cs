using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement.Abstractions;

namespace Bw.UseCases.Combat.Abstractions.Characters
{
    public interface IReadonlyCharacter
    {
        public IReadonlyMortal Mortal { get; }
        public IReadonlyMovement Movement { get; }
        public IReadonlyHands Hands { get; }
        public IReadonlyWeaponInventory Inventory { get; }
    }

    public interface ICharacter : IReadonlyCharacter
    {
        public new IWeaponInventory Inventory { get; }
    }
}
