using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Pickup.Abstractions
{
    public interface IReadonlyFloorWeapons
    {
        public IReadOnlyList<IFloorWeapon> Lying { get; }
    }

    public interface IFloorWeapons : IReadonlyFloorWeapons
    {
        public void AddLifetimed(Lifetime lifetime, IFloorWeapon weapon);
    }
}
