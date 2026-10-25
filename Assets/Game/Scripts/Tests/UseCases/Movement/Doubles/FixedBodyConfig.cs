using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Doubles
{
    public sealed class FixedBodyConfig : IBodyConfig
    {
        public float GravityScale { get; set; } = 1f;
        public LayerMask GroundLayer { get; set; } = 1 << 14;
        public float PushDeceleration { get; set; } = 10f;
        public float AirPushDeceleration { get; set; } = 5f;
        public float PushLift { get; set; } = 1f;
        public float ShoveForce { get; set; } = 80f;
    }
}
