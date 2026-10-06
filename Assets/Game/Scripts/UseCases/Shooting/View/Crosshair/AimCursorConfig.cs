using UnityEngine;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    [CreateAssetMenu(fileName = "AimCursorConfig", menuName = "Configs/AimCursorConfig")]
    public sealed class AimCursorConfig : ScriptableObject
    {
        [Header("Aim To Reload")]
        [Min(0.01f)] public float TransitionTime = 0.22f;
        public float TickRetract = 0.5f;
        public float TickTwist = 45f;
        public float RingSwell = 1.12f;
        public float DialGrow = 0.82f;
        public float Overshoot = 1.7f;

        [Header("Reloading")]
        public float WobbleAngle = 7f;
        [Min(0.01f)] public float WobbleTime = 0.35f;

        [Header("Reloaded")]
        public float PunchScale = 1.18f;
        [Min(0.01f)] public float PunchTime = 0.2f;
    }
}
