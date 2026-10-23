using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Hands.Abstractions
{
    public interface IHeldBoxes
    {
        public IViewableMap<IWeapon, IHeldBox> ByWeapon { get; }
    }
}
