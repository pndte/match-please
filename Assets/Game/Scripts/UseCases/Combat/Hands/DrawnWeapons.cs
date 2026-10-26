using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class DrawnWeapons
    {
        public IReadonlyViewableList<IWeapon> Drawn => _drawn;

        private readonly BwViewableList<IWeapon> _drawn = new();
        private readonly Signal<Unit> _stowing = new();
        private readonly Signal<Unit> _drawing = new();
        private readonly IReadonlyHands _hands;

        public DrawnWeapons(Lifetime lifetime, IReadonlyWeaponInventory inventory, IReadonlyHands hands)
        {
            _hands = hands;

            inventory.Carried.View(lifetime, (carriedLifetime, weapon, _) => Carry(carriedLifetime, weapon));
            hands.State.Advise(lifetime, _ => Redraw());
        }

        private void Carry(Lifetime carriedLifetime, IWeapon weapon)
        {
            carriedLifetime.OnTermination(Redraw);
            var drawn = new ViewableProperty<bool>(false);
            _stowing.Advise(carriedLifetime, _ =>
            {
                if (!InHands(weapon))
                    drawn.Value = false;
            });
            _drawing.Advise(carriedLifetime, _ => drawn.Value = InHands(weapon));
            drawn.WhenTrue(carriedLifetime, drawnLifetime => Draw(drawnLifetime, weapon));
            Redraw();
        }

        private void Draw(Lifetime drawnLifetime, IWeapon weapon) =>
            _drawn.AddLifetimed(drawnLifetime, weapon);

        private void Redraw()
        {
            _stowing.Fire(Unit.Instance);
            _drawing.Fire(Unit.Instance);
        }

        private bool InHands(IWeapon weapon) =>
            _hands.TryFindInHands(out var inHands) && inHands == weapon;
    }
}
