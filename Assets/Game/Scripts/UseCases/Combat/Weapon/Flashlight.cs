using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class Flashlight : IFlashlight
    {
        public IReadonlyProperty<bool> Lit => _lit;

        private readonly ViewableProperty<bool> _lit = new(false);

        public void Apply(bool lit) =>
            _lit.Value = lit;
    }
}
