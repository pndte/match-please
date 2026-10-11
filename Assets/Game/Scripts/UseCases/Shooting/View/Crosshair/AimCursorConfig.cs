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

        [Header("Trigger Held")]
        public float PressScale = 0.86f;
        [Min(0.01f)] public float PressTime = 0.06f;
        [Min(0.01f)] public float ReleaseTime = 0.28f;
        public float ReleaseOvershoot = 2.6f;

        [Header("Shot")]
        public float KickScale = 0.18f;
        public float KickAngle = 6f;
        public float KickShake = 2.5f;
        [Min(1)] public int KickVibrato = 40;
        [Min(0.01f)] public float KickTime = 0.1f;

        [Header("Aim To Fists")]
        [Min(0.01f)] public float FistsTime = 0.24f;
        public float FistsSlide = 14f;
        public float FistsGrow = 0.6f;

        [Header("Jab")]
        public float JabReach = 5f;
        public float JabScale = 0.2f;
        public float JabKick = 0.08f;
        [Min(0.01f)] public float JabTime = 0.16f;

        [Header("Rounds")]
        [Range(0f, 1f)] public float RoundsLowShare = 0.25f;
        [Min(0.01f)] public float RoundsShowTime = 0.2f;
        [Min(0.01f)] public float RoundsHideTime = 0.12f;
        public float RoundsTuck = 0.55f;
        public float RoundsTickScale = 0.22f;
        public float RoundsTickDrop = 2.5f;
        [Min(0.01f)] public float RoundsTickTime = 0.14f;
        [Min(0.01f)] public float RoundsPopTime = 0.28f;
        public float RoundsPulseScale = 1.12f;
        [Min(0.01f)] public float RoundsPulseTime = 0.32f;
    }
}
