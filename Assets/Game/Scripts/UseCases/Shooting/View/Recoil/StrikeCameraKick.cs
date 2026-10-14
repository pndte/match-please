using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Impact;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Recoil
{
    public sealed class StrikeCameraKick
    {
        public StrikeCameraKick(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IStrikeImpacts impacts,
            ICameraShake cameraShake,
            StrikeFeelConfig config)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                impacts.Bodies.Advise(mineLifetime, impact => cameraShake.Kick(impact.Direction * config.CameraKick)));
        }
    }
}
