using Bw.Entities.Extensions;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponDrawBroadcaster //TODO: смена оружия — два сообщения двух объектов (сначала «убрано» у старого, потом «достано» у нового); надёжный канал упорядочен, так что двух оружий в руках не будет, но если сообщения придут в разных кадрах, на кадр руки окажутся пустыми; атомарно — одно сообщение персонажа о том, что у него в руках
    {
        public WeaponDrawBroadcaster(Lifetime lifetime, IReadonlyWeaponDraw draw, INetResultSender<bool> drawn)
        {
            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                drawn.Broadcast(true);
                drawnLifetime.OnTermination(() => drawn.Broadcast(false));
            });
        }
    }
}
