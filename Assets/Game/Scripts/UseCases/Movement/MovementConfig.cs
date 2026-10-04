using UnityEngine;

namespace Bw.UseCases.Movement
{
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Configs/MovementConfig")]
    public class MovementConfig : ScriptableObject
    {
        public float Speed;
        public float JumpForce;
        public float GravityScale = 3f;
        public LayerMask GroundLayer;
    }
}