using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface IAimDeflections
    {
        public IViewableMap<IWeapon, IReadonlyAimDeflection> ByWeapon { get; }
    }
}
