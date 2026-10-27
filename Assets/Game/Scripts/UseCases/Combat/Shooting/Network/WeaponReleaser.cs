using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Movement;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Shooting.Network
{
    public sealed class WeaponReleaser
    {
        private readonly IWeapon _weapon;
        private readonly IReadonlyWeaponCarriers _carriers;
        private readonly WeaponReleaseSpots _spots;
        private readonly IRestartable<RigidState> _body;

        public WeaponReleaser(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IWeapon weapon,
            IReadonlyWeaponCarriers carriers,
            WeaponReleaseSpots spots,
            IRestartable<RigidState> body)
        {
            _weapon = weapon;
            _carriers = carriers;
            _spots = spots;
            _body = body;

            networkObject.SpawnedLifetime.WhenAlive(lifetime, Watch);
        }

        private void Watch(Lifetime spawnedLifetime) =>
            _carriers.ByWeapon.ViewForKey(spawnedLifetime, _weapon, (carriedLifetime, carrier) =>
                carriedLifetime.OnTermination(() =>
                {
                    if (spawnedLifetime.IsAlive)
                        Release(carrier);
                }));

        private void Release(IReadonlyCharacter carrier) =>
            _body.Restart(_spots.ReleasedFrom(carrier.Hands.Pose));
    }
}
