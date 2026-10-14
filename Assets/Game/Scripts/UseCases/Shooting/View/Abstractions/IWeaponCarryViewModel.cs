using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface IWeaponCarryViewModel
    {
        public IReadonlyProperty<bool> Held { get; }
        public IReadonlyProperty<bool> Drawn { get; }
        public IReadonlyProperty<bool> Mine { get; }
    }
}
