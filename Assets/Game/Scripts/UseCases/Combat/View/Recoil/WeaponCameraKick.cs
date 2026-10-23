using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Recoil
{
    public sealed class WeaponCameraKick
    {
        public WeaponCameraKick(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IShotsViewModel shots,
            ICameraShake cameraShake,
            WeaponCameraKickConfig config)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                shots.Seen.Advise(mineLifetime, shot => cameraShake.Kick(-shot.Direction * config.Kick)));
        }
    }
}
