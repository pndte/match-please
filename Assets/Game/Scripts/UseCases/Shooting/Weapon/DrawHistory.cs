using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class DrawHistory
    {
        public IReadOnlyList<IWeapon> Order => _order;

        private readonly List<IWeapon> _order = new();

        public DrawHistory(
            Lifetime lifetime,
            IReadonlyMortal character,
            IViewableBiMap<IReadonlyMortal, IWeapon> drawn,
            IViewableMap<IWeapon, IReadonlyMortal> carriers)
        {
            drawn.ViewForKey(lifetime, character, (_, weapon) => Remember(weapon));
            carriers.View(lifetime, (carryLifetime, weapon, carrier) =>
            {
                if (carrier == character)
                    carryLifetime.OnTermination(() => _order.Remove(weapon));
            });
        }

        private void Remember(IWeapon weapon)
        {
            _order.Remove(weapon);
            _order.Add(weapon);
        }
    }
}
