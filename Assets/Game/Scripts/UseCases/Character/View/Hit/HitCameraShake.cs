using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Camera.View.Shake;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.View.Hit
{
    public sealed class HitCameraShake
    {
        public HitCameraShake(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyHealth health,
            ICameraShake cameraShake,
            HitFeedbackConfig config)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                health.AdviseDamage(controlledLifetime, _ => cameraShake.Shake(config.CameraShake)));
        }
    }
}
