using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class AimDeflections : IAimDeflections
    {
        public IViewableMap<IWeapon, IReadonlyAimDeflection> ByWeapon { get; } = new ViewableMap<IWeapon, IReadonlyAimDeflection>();
    }
}
