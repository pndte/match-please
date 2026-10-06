using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDropServerHandler
    {
        public WeaponDropServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponDropRequest dropRequest,
            IHeldWeaponCollection heldWeapons)
        {
            dropRequest.Requested.Advise(lifetime, _ =>
            {
                if (!heldWeapons.ByCharacter.RemoveRight(weapon))
                    throw new InvalidOperationException(
                        "Drop request for a weapon nobody holds: the receive guard admits it only from the owner, and only a held weapon has one.");
            });
        }
    }
}
