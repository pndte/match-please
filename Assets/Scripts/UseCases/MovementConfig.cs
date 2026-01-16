using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Configs/MovementConfig")]
    public class MovementConfig : ScriptableObject
    {
        public float Speed;
        public float JumpForce;
    }
}