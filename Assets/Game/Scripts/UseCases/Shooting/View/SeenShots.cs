using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View
{
    public sealed class SeenShots : IStateView<WeaponState>, ISeenShots
    {
        public ISource<float> Seen => _seen;

        private readonly Signal<float> _seen = new();

        private bool _held;
        private bool _remote;
        private bool _synchronized;
        private int _shots;

        public SeenShots(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeapon weapon,
            IWeaponHold hold)
        {
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                _held = true;
                heldLifetime.OnTermination(() => _held = false);
            });
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, shot => _seen.Fire(shot.Aim)));
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

            var fired = _synchronized && to.Shots > _shots; //TODO: несколько выстрелов между двумя кадрами (низкий FPS) видны как один, с последним стволом: отдача и трассер проиграются один раз
            _shots = to.Shots;
            _synchronized = true;

            if (fired && _held)
                _seen.Fire(to.Aim);
        }
    }
}
