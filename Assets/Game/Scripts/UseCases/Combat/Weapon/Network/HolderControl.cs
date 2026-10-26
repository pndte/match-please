using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class HolderControl : IReadonlyControlledBy
    {
        public IReadonlyProperty<bool> Me => _me;

        private readonly ViewableProperty<bool> _me = new(false);

        public HolderControl(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyWeaponsInHands inHands,
            IControlledCharacters controlled)
        {
            inHands.ByWeapon.ViewForKey(lifetime, weapon, (drawnLifetime, holder) =>
                controlled.View(drawnLifetime, (controlledLifetime, character) =>
                {
                    if (character == holder)
                        Raise(controlledLifetime);
                }));
        }

        private void Raise(Lifetime controlledLifetime)
        {
            _me.Value = true;
            controlledLifetime.OnTermination(() => _me.Value = false);
        }
    }
}
