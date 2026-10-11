using UnityEngine;

namespace Bw.UseCases.FogOfWar.View
{
    [CreateAssetMenu(fileName = "FogOfWarConfig", menuName = "Configs/FogOfWarConfig")]
    public class FogOfWarConfig : ScriptableObject
    {
        [Header("Sight")]
        public LayerMask Blockers;
        public Vector2 EyeOffset = new(0f, 0.6f);
        [Min(0)] public int FanRays = 64;
        [Min(0f)] public float ViewMargin = 1.5f;

        [Header("Walls")]
        [Min(0f)] public float WallDepth = 0.6f;
        [Min(0f)] public float HidingDepth = 0.15f;

        [Header("Fog")]
        public Color FogColor = new(0.9f, 0.88f, 0.84f, 0.9f);
        [Min(0f)] public float Softness = 0.35f;
        [Range(0f, 1f)] public float Grain = 0.1f;

        [Header("Hatching")]
        public Color HatchColor = new(0.3f, 0.3f, 0.35f, 0.32f);
        public float HatchAngle = 45f;
        [Min(0.02f)] public float HatchSpacing = 0.13f;
        [Range(0.05f, 1f)] public float HatchWidth = 0.32f;
        [Min(0.05f)] public float HatchLength = 1.3f;

        [Header("Mask")]
        [Min(1)] public int MaskTexelsPerUnit = 8;

        [Header("Sorting")]
        public int HidingOrder = -3;
        public int FogOrder = 900;

        [Header("Shaders")]
        public Shader MaskShader;
        public Shader HidingShader;
        public Shader FogShader;
    }
}
