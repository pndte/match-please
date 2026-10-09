using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Rig
{
    public sealed class WeaponRigView : IStateView<WeaponState>
    {
        private static readonly int HeldId = Animator.StringToHash(WeaponRigParameters.Held);
        private static readonly int ReloadingId = Animator.StringToHash(WeaponRigParameters.Reloading);
        private static readonly int EmptyId = Animator.StringToHash(WeaponRigParameters.Empty);
        private static readonly int ReloadProgressId = Animator.StringToHash(WeaponRigParameters.ReloadProgress);

        private readonly Animator _rig;
        private readonly ISimulationStep _step;
        private readonly ShootingWeaponConfig _config;

        private bool _reloading;

        public WeaponRigView(
            Lifetime lifetime,
            Animator rig,
            IWeaponHold hold,
            ISimulationStep step,
            ShootingWeaponConfig config)
        {
            _rig = rig;
            _step = step;
            _config = config;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                rig.SetBool(HeldId, true);
                heldLifetime.OnTermination(() => rig.SetBool(HeldId, false));
            });
        }

        public void Show(WeaponState from, WeaponState to, float progress)
        {
            var ticksLeft = from.ReloadTicks > 0 && to.ReloadTicks > 0
                ? Mathf.Lerp(from.ReloadTicks, to.ReloadTicks, progress)
                : to.ReloadTicks;
            var reloading = ticksLeft > 0f;

            if (reloading && !_reloading)
                _rig.SetBool(EmptyId, to.Ammo == 0);
            if (reloading)
                _rig.SetFloat(ReloadProgressId, 1f - ticksLeft / ReloadTicks());
            if (reloading != _reloading)
                _rig.SetBool(ReloadingId, reloading);

            _reloading = reloading;
        }

        private float ReloadTicks() =>
            Mathf.Max(1, Mathf.CeilToInt(_config.ReloadTime / _step.Duration));
    }
}
