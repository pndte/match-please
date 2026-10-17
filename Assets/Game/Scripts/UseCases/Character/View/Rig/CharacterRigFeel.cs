using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    [CreateAssetMenu(fileName = "CharacterRigFeel", menuName = "Configs/CharacterRigFeel")]
    public sealed class CharacterRigFeel : ScriptableObject
    {
        public bool Held = false;
        public float HoldsPerSecond = 12f;
        public float LinesPerSecond = 12f;
        public float LiftStretch = 0.08f;
        public float LiftDecay = 0.07f;
        public float LandSquash = 0.17f;
        public float LandDecay = 0.12f;
        public float LandFrequency = 4.2f;
        public float FullImpact = 1.1f;
        public float HairFrequency = 3.2f;
        public float HairDamping = 0.35f;
        public Vector2 HairFollow = new(0.26f, 0.32f);
        public float HairLimit = 12f;
        public float HairSnap = 100f;
        public string GroundedParameter = "Grounded";
        public string VerticalSpeedParameter = "VerticalSpeed";
    }
}
