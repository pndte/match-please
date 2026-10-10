using System;
using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class MovementConfigExtensions
    {
        public static int CoyoteTicks(this MovementConfig config, ISimulationStep step) =>
            Ticks(config.CoyoteTime, step);

        public static int JumpBufferTicks(this MovementConfig config, ISimulationStep step) =>
            Ticks(config.JumpBufferTime, step);

        public static float DeceleratedPush(this MovementConfig config, float push, ISimulationStep step) =>
            Mathf.MoveTowards(push, 0f, config.PushDeceleration * step.Duration);

        public static int PushTicks(this MovementConfig config, float push, ISimulationStep step)
        {
            RequireStopping(config, push);
            var ticks = 0;
            for (; push != 0f; ticks++)
                push = config.DeceleratedPush(push, step);

            return ticks;
        }

        public static float PushDistance(this MovementConfig config, float push, double ticks, ISimulationStep step)
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

        private static void RequireStopping(MovementConfig config, float push)
        {
            if (config.PushDeceleration <= 0f || !float.IsFinite(push))
                throw new ArgumentException(
                    $"A push of {push.ToString()} u/s at a deceleration of {config.PushDeceleration.ToString()} u/s² never stops: the push must be finite and the deceleration positive.");
        }

        private static int Ticks(float seconds, ISimulationStep step) =>
            Mathf.RoundToInt(seconds / step.Duration);
    }
}
