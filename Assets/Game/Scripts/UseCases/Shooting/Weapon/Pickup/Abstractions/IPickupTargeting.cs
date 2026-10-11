using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Pickup.Abstractions
{
    public interface IPickupTargeting
    {
        public IReadonlyProperty<PickupTarget> Target { get; }
    }
}
