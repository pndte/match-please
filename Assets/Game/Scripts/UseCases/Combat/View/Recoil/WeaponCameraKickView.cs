using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Recoil
{
    public sealed class WeaponCameraKickView
    {
        public WeaponCameraKickView(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IShotsViewModel shots,
            ICameraShaker cameraShaker,
            WeaponCameraKickConfig config)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                shots.Seen.Advise(mineLifetime, shot => cameraShaker.Kick(-shot.Direction * config.Kick)));
        }
    }
}
