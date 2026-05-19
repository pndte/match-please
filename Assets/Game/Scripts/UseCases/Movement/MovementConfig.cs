using UnityEngine;

namespace Bw.UseCases.Movement
{
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Configs/MovementConfig")]
    public class MovementConfig : ScriptableObject
    {
        public float Speed;
        public float JumpForce;
        public LayerMask GroundLayer;
    }
}