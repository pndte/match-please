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
            var toward = to - from;
            gamepad.AimAt(Mathf.Atan2(toward.y, toward.x) * Mathf.Rad2Deg + offset);
        }
    }
}
