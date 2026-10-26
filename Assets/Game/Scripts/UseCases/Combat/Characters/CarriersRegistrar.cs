using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Characters
{
    public sealed class CarriersRegistrar
    {
        public CarriersRegistrar(Lifetime lifetime, ICharacter character, IWeaponCarriers carriers)
        {
            character.Inventory.Carried.View(lifetime, (carriedLifetime, weapon, _) =>
                carriers.Add(carriedLifetime, weapon, character));
        }
    }
}
