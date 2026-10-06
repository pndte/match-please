using Bw.Entities;
using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Recoil
{
    public sealed class WeaponCameraKick
    {
        public WeaponCameraKick(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeapon weapon,
            IShotTracer tracer,
            ICameraShake cameraShake,
            WeaponCameraKickConfig config)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, shot =>
                    cameraShake.Kick(-tracer.Aim(shot.Aim).Direction * config.Kick)));
        }
    }
}
