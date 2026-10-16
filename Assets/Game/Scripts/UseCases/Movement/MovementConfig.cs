using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Configs/MovementConfig")]
    public class MovementConfig : ScriptableObject, IBodyConfig
    {
        public float Speed;
        public float JumpForce;
        public float GravityScale = 3f;
        public LayerMask GroundLayer;

        [Header("Jump assist")]
        [Min(0f)] public float CoyoteTime = 0.1f;
        [Min(0f)] public float JumpBufferTime = 0.1f;

        [Header("Variable jump")]
        [Min(1f)] public float ReleasedJumpGravity = 3f;

        [Header("Fall")]
        [Min(1f)] public float FallGravity = 1f;

        [Header("Push")]
        [Min(0f)] public float PushDeceleration = 40f;
        [Min(0f)] public float PushLift = 1f;

        [Header("Shove")]
        [Min(0f)] public float ShoveForce = 80f;

        float IBodyConfig.GravityScale => GravityScale;
        LayerMask IBodyConfig.GroundLayer => GroundLayer;
        float IBodyConfig.PushDeceleration => PushDeceleration;
        float IBodyConfig.AirPushDeceleration => PushDeceleration;
        float IBodyConfig.PushLift => PushLift;
        float IBodyConfig.ShoveForce => ShoveForce;
    }
}
