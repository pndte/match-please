using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Character.View.Player;
using Bw.UseCases.Mortality.View.Hit;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.View.Hit
{
    public sealed class HitCameraShakeView
    {
        public HitCameraShakeView(
            Lifetime lifetime,
            IPlayerViewModel player,
            IDamageViewModel damage,
            ICameraShaker cameraShaker,
            HitFeedbackConfig config)
        {
            player.Mine.WhenTrue(lifetime, mineLifetime =>
                damage.Taken.Advise(mineLifetime, _ => cameraShaker.Shake(config.CameraShake)));
        }
    }
}
