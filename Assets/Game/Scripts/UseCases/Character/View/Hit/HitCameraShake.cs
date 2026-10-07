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
            IOwnership ownership,
            IReadonlyHealth health,
            ICameraShake cameraShake,
            HitFeedbackConfig config)
        {
            ownership.Mine.WhenTrue(lifetime, mineLifetime =>
                health.AdviseDamage(mineLifetime, _ => cameraShake.Shake(config.CameraShake)));
        }
    }
}
