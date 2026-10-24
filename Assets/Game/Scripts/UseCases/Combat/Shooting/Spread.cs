using Bw.UseCases.Combat.Shooting.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Combat.Shooting
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

        public float Returned(float deflection, float seconds) =>
            Mathf.MoveTowards(deflection, 0f, _config.Spread.Return * seconds);

        private static float UpOf(float aim) => //TODO: у вертикального ствола подброс скачет: «вверх» считается по стороне, куда смотрит ствол, и у самой вертикали меняет знак
            Mathf.Abs(Mathf.DeltaAngle(0f, aim)) > 90f ? -1f : 1f;
    }
}
