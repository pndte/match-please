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
    }
}
