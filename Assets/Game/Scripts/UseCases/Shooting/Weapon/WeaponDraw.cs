using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponDraw : IWeaponDraw
    {
        public IReadonlyProperty<Lifetime> DrawnLifetime => _drawnLifetime;

        private readonly ViewableProperty<Lifetime> _drawnLifetime = new(Lifetime.Terminated);

        public void Draw(Lifetime drawnLifetime)
        {
            if (_drawnLifetime.Value.IsAlive)
                throw new InvalidOperationException("A weapon is drawn once at a time: the current draw must end before the next one.");

            _drawnLifetime.Value = drawnLifetime;
            drawnLifetime.OnTermination(() => _drawnLifetime.Value = Lifetime.Terminated);
        }
    }
}
