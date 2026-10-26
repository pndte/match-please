using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Rollback;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponPredictedBy : IPredictedBy
    {
        public IReadonlyProperty<bool> Me => _me;

        private readonly ViewableProperty<bool> _me = new(false);
        private readonly IBubbleMembership _floor;

        private bool _carried;
        private bool _holderPredicted;

        public WeaponPredictedBy(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyWeaponCarriers carriers,
            ICharacterPredictions characters,
            IBubbleMembership floor)
        {
            _floor = floor;

            floor.Inside.Advise(lifetime, _ => Settle());
            carriers.ByWeapon.ViewForKey(lifetime, weapon, (carriedLifetime, carrier) =>
            {
                _carried = true;
                carriedLifetime.OnTermination(() =>
                {
                    _carried = false;
                    _holderPredicted = false;
                    Settle();
                });

                characters.ByCharacter.ViewForKey(carriedLifetime, carrier, (registeredLifetime, holder) =>
                    holder.Me.Advise(registeredLifetime, predicted =>
                    {
                        _holderPredicted = predicted;
                        Settle();
                    }));

                Settle();
            });
        }

        private void Settle() =>
            _me.Value = _carried ? _holderPredicted : _floor.Inside.Value;
    }
}
