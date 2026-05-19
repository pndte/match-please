using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReadonlyAmmo : IReadonlyProperty<int>
    {
        public int Max { get; }
    }
    
    public interface IAmmo : IReadonlyAmmo, IViewableProperty<int>
    {
    }

}