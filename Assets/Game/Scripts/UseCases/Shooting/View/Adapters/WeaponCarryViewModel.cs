using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Adapters
{
    public sealed class WeaponCarryViewModel : IWeaponCarryViewModel
    {
        public IReadonlyProperty<bool> Held => _held;
        public IReadonlyProperty<bool> Drawn => _drawn;
        public IReadonlyProperty<bool> Mine => _controlledBy.Me;

        private readonly ViewableProperty<bool> _held = new(false);
        private readonly ViewableProperty<bool> _drawn = new(false);
        private readonly IReadonlyControlledBy _controlledBy;

        public WeaponCarryViewModel(
            Lifetime lifetime,
            IWeaponHold hold,
            IReadonlyWeaponDraw draw,
            IReadonlyControlledBy controlledBy)
        {
            _controlledBy = controlledBy;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime => Raise(_held, heldLifetime));
            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime => Raise(_drawn, drawnLifetime));
        }

        private static void Raise(ViewableProperty<bool> flag, Lifetime raisedLifetime)
        {
            flag.Value = true;
            raisedLifetime.OnTermination(() => flag.Value = false);
        }
    }
}
