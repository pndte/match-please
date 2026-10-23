using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class HeldBoxes : IHeldBoxes
    {
        public IViewableMap<IWeapon, IHeldBox> ByWeapon { get; } = new ViewableMap<IWeapon, IHeldBox>();
    }
}
