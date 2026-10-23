using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Pickup.Abstractions
{
    public interface IPickupTargeting
    {
        public IReadonlyProperty<PickupTarget> Target { get; }
    }
}
