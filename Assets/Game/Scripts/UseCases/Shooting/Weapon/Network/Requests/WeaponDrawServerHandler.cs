using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDrawServerHandler
    {
        public WeaponDrawServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            INetRequestReceiver<Unit> draw,
            IWeaponInventories inventories)
        {
            draw.Received.Advise(lifetime, _ => inventories.Draw(weapon));
        }
    }
}
