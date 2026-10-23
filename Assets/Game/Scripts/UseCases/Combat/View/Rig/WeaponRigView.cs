using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Rig
{
    public sealed class WeaponRigView
    {
        private static readonly int HeldId = Animator.StringToHash(WeaponRigParameters.Held);
        private static readonly int ReloadingId = Animator.StringToHash(WeaponRigParameters.Reloading);
        private static readonly int EmptyId = Animator.StringToHash(WeaponRigParameters.Empty);
        private static readonly int ReloadProgressId = Animator.StringToHash(WeaponRigParameters.ReloadProgress);

        private readonly Animator _rig;

        private bool _reloading;

        public WeaponRigView(
            Lifetime lifetime,
            Animator rig,
            IWeaponCarryViewModel carry,
            IGunViewModel gun)
        {
            _rig = rig;

            carry.Drawn.WhenTrue(lifetime, drawnLifetime =>
            {
                rig.SetBool(HeldId, true);
                drawnLifetime.OnTermination(() => rig.SetBool(HeldId, false));
            });
            gun.Reload.Advise(lifetime, Follow);
        }

        private void Follow(GunReload reload)
        {
            if (reload.Active && !_reloading)
                _rig.SetBool(EmptyId, reload.Empty);

            if (reload.Active)
                _rig.SetFloat(ReloadProgressId, reload.Progress);

            if (reload.Active != _reloading)
                _rig.SetBool(ReloadingId, reload.Active);

            _reloading = reload.Active;
        }
    }
}
