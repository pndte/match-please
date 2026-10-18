using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Props.View.Abstractions;
using Bw.UseCases.Shooting.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.View.Throw
{
    public sealed class ThrowCameraKick
    {
        public ThrowCameraKick(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IPropThrowsViewModel throws,
            ICameraShake cameraShake,
            PropVfxConfig config)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                throws.Thrown.Advise(mineLifetime, thrown => cameraShake.Kick(thrown.Direction * config.ThrowKick)));
        }
    }
}
