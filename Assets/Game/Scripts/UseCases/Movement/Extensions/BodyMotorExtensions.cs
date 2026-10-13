using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class BodyMotorExtensions
    {
        public static BodyState Step(this IBodyMotor motor, BodyState state, Vector2 shoved) =>
            motor.Step(state.Shifted(shoved), BodyDrive.Free(state), out _);
    }
}
