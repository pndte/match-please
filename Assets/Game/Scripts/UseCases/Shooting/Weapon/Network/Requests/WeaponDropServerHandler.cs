using System;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDropServerHandler
    {
        public WeaponDropServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            INetRequestReceiver<Unit> drop,
            IHeldWeaponCollection heldWeapons)
        {
            drop.Received.Advise(lifetime, _ =>
            {
                if (!heldWeapons.ByCharacter.RemoveRight(weapon))
                    throw new InvalidOperationException(
                        "Drop request for a weapon nobody holds: the receive guard admits it only from the owner, and only a held weapon has one.");
            });
        }
    }
}
