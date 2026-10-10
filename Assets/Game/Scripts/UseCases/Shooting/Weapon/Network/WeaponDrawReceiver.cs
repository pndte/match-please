using Bw.Entities.Extensions;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponDrawReceiver
    {
        private readonly ViewableProperty<bool> _drawn = new(false);

        public WeaponDrawReceiver(
            Lifetime lifetime,
            IWeaponHold hold,
            IWeaponDraw draw,
            INetResultReceiver<bool> drawn)
        {
            drawn.Received.Advise(lifetime, value => _drawn.Value = value);
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
                _drawn.WhenTrue(heldLifetime, draw.Draw));
        }
    }
}
