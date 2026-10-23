using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IWeaponFeed
    {
        public ISource<WeaponInput> Fed { get; }
    }
}
