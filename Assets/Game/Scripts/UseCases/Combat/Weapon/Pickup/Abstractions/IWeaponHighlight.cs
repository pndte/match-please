using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Pickup.Abstractions
{
    public interface IWeaponHighlight
    {
        public void Show(Lifetime highlightLifetime);
    }
}
