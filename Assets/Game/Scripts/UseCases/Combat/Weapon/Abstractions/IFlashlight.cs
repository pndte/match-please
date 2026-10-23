using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IReadonlyFlashlight
    {
        public IReadonlyProperty<bool> Lit { get; }
    }

    public interface IFlashlight : IReadonlyFlashlight
    {
        public void Apply(bool lit);
    }
}
