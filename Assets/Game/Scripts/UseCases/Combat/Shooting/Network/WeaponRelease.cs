using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Movement;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Shooting.Network
{
    public sealed class WeaponRelease
    {
        private readonly WeaponReleaseSpots _spots;
        private readonly IRestartable<RigidState> _body;

        public WeaponRelease(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponCarriers carriers,
            WeaponReleaseSpots spots,
            IRestartable<RigidState> body)
        {
            _spots = spots;
            _body = body;

            carriers.ByWeapon.ViewForKey(lifetime, weapon, (carriedLifetime, carrier) =>
                carriedLifetime.OnTermination(() =>
                {
                    if (lifetime.IsAlive)
                        Release(carrier);
                }));
        }

        private void Release(IReadonlyCharacter carrier) =>
            _body.Restart(_spots.ReleasedFrom(carrier.Hands.Pose));
    }
}
