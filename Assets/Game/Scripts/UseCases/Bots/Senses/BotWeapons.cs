using System;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Props;

namespace Bw.UseCases.Bots.Senses
{
    public sealed class BotWeapons
    {
        private readonly IReadonlyWeaponsInHands _inHands;
        private readonly IReadonlyWeaponStates<GunState> _guns;
        private readonly IReadonlyWeaponStates<MeleeState> _blows;
        private readonly IReadonlyWeaponStates<PropState> _throws;

        public BotWeapons(
            IReadonlyWeaponsInHands inHands,
            IReadonlyWeaponStates<GunState> guns,
            IReadonlyWeaponStates<MeleeState> blows,
            IReadonlyWeaponStates<PropState> throws)
        {
            _inHands = inHands;
            _guns = guns;
            _blows = blows;
            _throws = throws;
        }

        public bool IsReloading(IReadonlyCharacter holder) =>
            holder.Hands.TryFindInHands(out var weapon)
            && holder.Inventory.RulesOf(weapon).Kind == WeaponKind.Gun
            && _guns.ByWeapon[weapon].State.Value.ReloadTicks > 0;

        public bool TryFindCharger(IReadonlyCharacter near, float range, out IReadonlyCharacter charger)
        {
            var position = near.Movement.State.Value.Body.Position;
            foreach (var (weapon, holder) in _inHands.ByWeapon)
                if (holder != near && IsCharging(weapon, holder) && (holder.Movement.State.Value.Body.Position - position).sqrMagnitude <= range * range)
                {
                    charger = holder;
                    return true;
                }

            charger = default;
            return false;
        }

        private bool IsCharging(IWeapon weapon, IReadonlyCharacter holder) =>
            holder.Inventory.RulesOf(weapon).Kind switch
            {
                WeaponKind.Fists => _blows.ByWeapon[weapon].State.Value.Phase == MeleePhase.Charging,
                WeaponKind.Prop => _throws.ByWeapon[weapon].State.Value.Phase == PropPhase.WindingUp,
                WeaponKind.Gun => false,
                _ => throw new ArgumentOutOfRangeException(nameof(weapon), holder.Inventory.RulesOf(weapon).Kind, "The bots know no such weapon kind.")
            };
    }
}
