using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Pickup.Abstractions
{
    public interface IPickupTargetSelector
    {
        public IReadonlyProperty<PickupTarget> Target { get; }
    }
}
