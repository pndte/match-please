using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class Spread : ISpread
    {
        private readonly ShootingWeaponConfig _config;

        public Spread(ShootingWeaponConfig config)
        {
            _config = config;
        }

        public float ThrowOf(float aim, float roll) =>
            UpOf(aim) * _config.Spread.Climb + roll * _config.Spread.Jitter;

        private static float UpOf(float aim) => //TODO: у вертикального ствола подброс скачет: «вверх» считается по стороне, куда смотрит ствол, и у самой вертикали меняет знак
            Mathf.Abs(Mathf.DeltaAngle(0f, aim)) > 90f ? -1f : 1f;
    }
}
