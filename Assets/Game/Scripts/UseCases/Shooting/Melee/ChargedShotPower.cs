using Bw.Entities.Simulation;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Melee.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Melee
{
    public sealed class ChargedShotPower : IShotPower<MeleeState>
    {
        private readonly ISimulationStep _step;
        private readonly MeleeWeaponConfig _config;

        public ChargedShotPower(ISimulationStep step, MeleeWeaponConfig config)
        {
            _step = step;
            _config = config;
        }

        public Hit Along(Vector2 direction, MeleeState state) =>
            state.Phase == MeleePhase.StrongSwing
                ? StrongHit(direction, _config.Charge(_step, state.ChargeTicks))
                : JabHit(direction);

        private Hit JabHit(Vector2 direction) =>
            new(_config.Damage, direction * _config.Knockback);

        private Hit StrongHit(Vector2 direction, float charge) =>
            new(
                Mathf.Lerp(_config.StrongBaseDamage, _config.StrongDamage, charge),
                direction * Mathf.Lerp(_config.StrongBaseKnockback, _config.StrongKnockback, charge));
    }
}
