using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReadonlyWeaponDraw
    {
        public IReadonlyProperty<Lifetime> DrawnLifetime { get; }
    }

    public interface IWeaponDraw : IReadonlyWeaponDraw
    {
        public void Draw(Lifetime drawnLifetime);
    }
}
