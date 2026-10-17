using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class BodyConfigExtensions
    {
        public static float DeceleratedPush(this IBodyConfig config, float push, ISimulationStep step) =>
            Mathf.MoveTowards(push, 0f, config.PushDeceleration * step.Duration);

        public static float DeceleratedPush(
            this IBodyConfig config,
            float push,
            bool grounded,
            ISimulationStep step) =>
            grounded ? config.DeceleratedPush(push, step) : Mathf.MoveTowards(push, 0f, config.AirPushDeceleration * step.Duration);

        public static Vector2 Lifted(this IBodyConfig config, Vector2 impulse) =>
            new(impulse.x, impulse.y * config.PushLift);
    }
}
