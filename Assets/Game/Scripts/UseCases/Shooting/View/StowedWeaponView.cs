using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    public sealed class StowedWeaponView
    {
        private readonly IWeaponHold _hold;
        private readonly IReadonlyWeaponDraw _draw;
        private readonly Renderer[] _renderers;

        public StowedWeaponView(Lifetime lifetime, IWeaponHold hold, IReadonlyWeaponDraw draw, Renderer[] renderers)
        {
            _hold = hold;
            _draw = draw;
            _renderers = renderers;

            hold.HeldLifetime.Advise(lifetime, _ => Refresh());
            draw.DrawnLifetime.Advise(lifetime, _ => Refresh());
        }

        private void Refresh()
        {
            var stowed = _hold.HeldLifetime.Value.IsAlive && !_draw.DrawnLifetime.Value.IsAlive;
            foreach (var renderer in _renderers)
                renderer.forceRenderingOff = stowed;
        }
    }
}
