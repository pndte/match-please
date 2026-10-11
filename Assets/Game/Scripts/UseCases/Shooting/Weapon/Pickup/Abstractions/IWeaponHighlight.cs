using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Pickup.Abstractions
{
    public interface IWeaponHighlight
    {
        public void Show(Lifetime highlightLifetime);
    }
}
