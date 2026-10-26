using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Impact;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Recoil
{
    public sealed class StrikeCameraKickView
    {
        public StrikeCameraKickView(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IStrikeImpacts impacts,
            ICameraShaker cameraShaker,
            StrikeFeelConfig config)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                impacts.Bodies.Advise(mineLifetime, impact => cameraShaker.Kick(impact.Direction * config.CameraKick)));
        }
    }
}
