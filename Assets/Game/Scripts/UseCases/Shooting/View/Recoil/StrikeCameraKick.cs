using Bw.Entities;
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
            IReadonlyControlledBy controlledBy,
            IStrikeImpacts impacts,
            ICameraShake cameraShake,
            StrikeFeelConfig config)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                impacts.Bodies.Advise(controlledLifetime, impact => cameraShake.Kick(impact.Direction * config.CameraKick)));
        }
    }
}
