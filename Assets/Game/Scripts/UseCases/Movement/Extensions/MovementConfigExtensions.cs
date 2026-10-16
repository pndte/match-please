using System;
using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class MovementConfigExtensions
    {
        private const int MaxRiseTicks = 600;

        public static int CoyoteTicks(this MovementConfig config, ISimulationStep step) =>
            Ticks(config.CoyoteTime, step);

        public static int JumpBufferTicks(this MovementConfig config, ISimulationStep step) =>
            Ticks(config.JumpBufferTime, step);

        public static float PushRise(
            this MovementConfig config,
            float lift,
            double ticks,
            ISimulationStep step) //TODO: подъём от толчка считается с той земли, где тело стояло, и без его собственной скорости по вертикали — толчок посреди прыжка или у края площадки стрелок покажет неточно, и снимки потом дотянут
        {
            var velocity = lift;
            var height = 0f;
            for (var tick = 0; tick < ticks; tick++)
            {
                velocity = Fallen(config, velocity, step);
                height += velocity * (float)Math.Min(1d, ticks - tick) * step.Duration;
                if (height <= 0f)
                    return 0f;
            }

            return height;
        }

        public static int PushRiseTicks(this MovementConfig config, float lift, ISimulationStep step)
        {
            var velocity = lift;
            var height = 0f;
            for (var ticks = 0; ticks < MaxRiseTicks; ticks++)
            {
                velocity = Fallen(config, velocity, step);
                height += velocity * step.Duration;
                if (height <= 0f)
                    return ticks + 1;
            }

            throw new InvalidOperationException(
                $"A lift of {lift.ToString()} u/s never lands within {MaxRiseTicks.ToString()} ticks: gravity must pull the body down.");
        }

        private static float Fallen(MovementConfig config, float velocity, ISimulationStep step) =>
            velocity + Physics2D.gravity.y * config.GravityScale * (velocity < 0f ? config.FallGravity : 1f) * step.Duration;

        private static int Ticks(float seconds, ISimulationStep step) =>
            Mathf.RoundToInt(seconds / step.Duration);
    }
}
