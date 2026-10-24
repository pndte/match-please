using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Combat.Shooting
{
    public sealed class FixedShotPower : IShotPower<GunState>
    {
        private readonly IHitConfig _config;

        public FixedShotPower(IHitConfig config)
        {
            _config = config;
        }

        public Hit Along(Vector2 direction, GunState state) =>
            Hit.Along(direction, _config);
    }
}
