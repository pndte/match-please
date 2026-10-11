using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class WeaponReloadCursor : IStateView<GunState>, IReloadTimer
    {
        public float Seconds => _ticks * _step.Duration;
        public float SecondsLeft { get; private set; }

        private readonly IAimCursor _cursor;
        private readonly ISimulationStep _step;
        private readonly SequentialLifetimes _reloads;

        private bool _shown;
        private bool _reloading;
        private int _ticks;

        public WeaponReloadCursor(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeaponDraw draw,
            IAimCursor cursor,
            ISimulationStep step)
        {
            _cursor = cursor;
            _step = step;
            _reloads = new SequentialLifetimes(lifetime);

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                draw.DrawnLifetime.WhenAlive(controlledLifetime, drawnLifetime =>
                {
                    _shown = true;
                    drawnLifetime.OnTermination(() =>
                    {
                        _shown = false;
                        Stop();
                    });
                }));
        }

        public void Show(GunState from, GunState to, float progress)
        {
            if (!_shown)
                return;

            var ticksLeft = from.ReloadTicks > 0 ? Mathf.Lerp(from.ReloadTicks, to.ReloadTicks, progress) : to.ReloadTicks;
            SecondsLeft = ticksLeft * _step.Duration;

            if (ticksLeft > 0f && !_reloading)
                Start(Mathf.Max(from.ReloadTicks, to.ReloadTicks));
            else if (ticksLeft <= 0f && _reloading)
                Stop();

            _ticks = Mathf.Max(_ticks, to.ReloadTicks);
        }

        private void Start(int ticks)
        {
            _reloading = true;
            _ticks = ticks;
            _cursor.ShowReload(_reloads.Next(), this);
        }

        private void Stop()
        {
            _reloading = false;
            _reloads.TerminateCurrent();
        }
    }
}
