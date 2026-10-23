using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class DrawnWeaponClients
    {
        public DrawnWeaponClients(
            Lifetime lifetime,
            HandsDraw draw,
            IControllingClients characterClients,
            IWeaponControllingClients weapons)
        {
            draw.Drawn.View(lifetime, (drawnLifetime, weapon) =>
                characterClients.View(drawnLifetime, (clientLifetime, client) =>
                    weapons.ByWeapon[weapon].Add(clientLifetime, client)));
        }
    }
}
