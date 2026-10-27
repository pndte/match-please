using Bw.UseCases.Bots.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Bots.Extensions
{
    public static class BotGamepadExtensions
    {
        public static void AimAt(
            this IBotGamepad gamepad,
            Vector2 from,
            Vector2 to,
            float offset)
        {
            gamepad.AimAt(from + (Vector2)(Quaternion.Euler(0f, 0f, offset) * (to - from)));
        }

        public static void RaiseGuard(this IBotGamepad gamepad, Vector2 from, Vector2 toward)
        {
            var direction = toward - from;
            gamepad.RaiseGuard(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
    }
}
