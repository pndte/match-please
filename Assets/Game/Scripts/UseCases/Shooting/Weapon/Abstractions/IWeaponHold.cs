using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeaponHold
    {
        public IReadonlyProperty<Lifetime> HeldLifetime { get; }
    }
}
