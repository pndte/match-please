using UnityEngine;

namespace Bw.UseCases.Movement.Abstractions
{
    public interface IBodyConfig
    {
        public float GravityScale { get; }
        public LayerMask GroundLayer { get; }
        public float PushDeceleration { get; }
        public float AirPushDeceleration { get; }
        public float PushLift { get; }
        public float ShoveSpeed { get; }
    }
}
