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

        [Header("Push")]
        [Min(0f)] public float PushDeceleration = 40f;

        [Header("Shove")]
        [Min(0f)] public float ShoveSpeed = 6f;

        float IBodyConfig.GravityScale => GravityScale;
        LayerMask IBodyConfig.GroundLayer => GroundLayer;
        float IBodyConfig.PushDeceleration => PushDeceleration;
    }
}
