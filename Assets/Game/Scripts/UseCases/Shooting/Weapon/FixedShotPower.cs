using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
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
