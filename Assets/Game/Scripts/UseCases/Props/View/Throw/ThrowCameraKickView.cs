using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Props.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.View.Throw
{
    public sealed class ThrowCameraKickView
    {
        public ThrowCameraKickView(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IPropThrowsViewModel throws,
            ICameraShaker cameraShaker,
            PropVfxConfig config)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                throws.Thrown.Advise(mineLifetime, thrown => cameraShaker.Kick(thrown.Direction * config.ThrowKick)));
        }
    }
}
