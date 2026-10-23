using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View
{
    public sealed class StowedWeaponView
    {
        private readonly IWeaponCarryViewModel _carry;
        private readonly Renderer[] _renderers;

        public StowedWeaponView(Lifetime lifetime, IWeaponCarryViewModel carry, Renderer[] renderers)
        {
            _carry = carry;
            _renderers = renderers;

            carry.Held.Advise(lifetime, _ => Refresh());
            carry.Drawn.Advise(lifetime, _ => Refresh());
        }

        private void Refresh()
        {
            var stowed = _carry.Held.Value && !_carry.Drawn.Value;
            foreach (var renderer in _renderers)
                renderer.forceRenderingOff = stowed;
        }
    }
}
