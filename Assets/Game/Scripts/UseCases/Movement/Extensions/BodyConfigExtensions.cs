using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class BodyConfigExtensions
    {
        public static float DeceleratedPush(this IBodyConfig config, float push, ISimulationStep step) =>
            Mathf.MoveTowards(push, 0f, config.PushDeceleration * step.Duration);

        public static float DeceleratedPush(this IBodyConfig config, float push, bool grounded, ISimulationStep step) =>
            grounded ? config.DeceleratedPush(push, step) : Mathf.MoveTowards(push, 0f, config.AirPushDeceleration * step.Duration);

        public static Vector2 Lifted(this IBodyConfig config, Vector2 impulse) =>
            new(impulse.x, impulse.y * config.PushLift);

        public static int PushTicks(this IBodyConfig config, float push, ISimulationStep step)
        {
            RequireStopping(config, push);
            var ticks = 0;
            for (; push != 0f; ticks++)
                push = config.DeceleratedPush(push, step);

            return ticks;
        }

        public static float PushDistance(this IBodyConfig config, float push, double ticks, ISimulationStep step) //TODO: путь толчка считается с торможением на земле, а в воздухе тело тормозит по AirPushDeceleration — толчок взлетевшего ящика стрелок покажет короче, чем его проведёт сервер, и снимки потом дотянут
        {
            RequireStopping(config, push);
            var distance = 0f;
            for (var tick = 0; tick < ticks && push != 0f; tick++)
            {
                distance += push * (float)Math.Min(1d, ticks - tick) * step.Duration;
                push = config.DeceleratedPush(push, step);
            }

            return distance;
        }

        private static void RequireStopping(IBodyConfig config, float push)
        {
            if (config.PushDeceleration <= 0f || !float.IsFinite(push))
                throw new ArgumentException(
                    $"A push of {push.ToString()} u/s at a deceleration of {config.PushDeceleration.ToString()} u/s² never stops: the push must be finite and the deceleration positive.");
        }
    }
}
