using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Graphics;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View
{
    public sealed class WeaponShotEffectsView : IStateView<WeaponState>
    {
        private readonly IShotTracer _tracer;
        private readonly IShotVfxPlayer _vfxPlayer;

        private bool _held;
        private bool _remote;
        private bool _synchronized;
        private int _shots;

        public WeaponShotEffectsView(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeapon weapon,
            IWeaponHold hold,
            IShotTracer tracer,
            IShotVfxPlayer vfxPlayer)
        {
            _tracer = tracer;
            _vfxPlayer = vfxPlayer;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                _held = true;
                heldLifetime.OnTermination(() => _held = false);
            });
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, shot => Play(shot.Aim)));
            controlledBy.Me.WhenFalse(lifetime, remoteLifetime =>
            {
                _remote = true;
                _synchronized = false;
                remoteLifetime.OnTermination(() => _remote = false);
            });
        }

        public void Show(WeaponState from, WeaponState to, float progress)
        {
            if (!_remote)
                return;

            var fired = _synchronized && to.Shots > _shots;
            _shots = to.Shots;
            _synchronized = true;

            if (fired && _held)
                Play(to.Aim);
        }

        private void Play(float aim)
        {
            var trace = _tracer.Trace(aim);
            if (trace.From != trace.To)
                _vfxPlayer.Play(trace.From, trace.To).Forget();
        }
    }
}
