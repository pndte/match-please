using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    [CreateAssetMenu(fileName = "BodyConfig", menuName = "Configs/BodyConfig")]
    public class BodyConfig : ScriptableObject, IBodyConfig
    {
        public float GravityScale = 6f;
        public LayerMask GroundLayer;

        [Header("Push")]
        [Min(0f)] public float PushDeceleration = 40f;
        [Min(0f)] public float AirPushDeceleration = 3f;
        [Min(0f)] public float PushLift = 1f;

        [Header("Shove")]
        [Min(0f)] public float ShoveSpeed;

        float IBodyConfig.GravityScale => GravityScale;
        LayerMask IBodyConfig.GroundLayer => GroundLayer;
        float IBodyConfig.PushDeceleration => PushDeceleration;
        float IBodyConfig.AirPushDeceleration => AirPushDeceleration;
        float IBodyConfig.PushLift => PushLift;
        float IBodyConfig.ShoveSpeed => ShoveSpeed;
    }
}
