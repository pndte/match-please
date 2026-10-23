using Bw.UseCases.Character.View.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public interface IWeaponCarryViewModel
    {
        public IReadonlyProperty<bool> Held { get; }
        public IReadonlyProperty<bool> Drawn { get; }
        public IReadonlyProperty<bool> Mine { get; }
        public ICharacterView Holder { get; }
    }
}
