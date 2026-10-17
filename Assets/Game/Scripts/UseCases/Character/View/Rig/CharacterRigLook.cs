using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    [CreateAssetMenu(fileName = "CharacterRigLook", menuName = "Configs/CharacterRigLook")]
    public sealed class CharacterRigLook : ScriptableObject
    {
        public RigTurnSprites[] Turns;
        public Sprite[] Eggs;
        public Sprite[] FarShoes;
        public Sprite[] NearShoes;
        public Sprite[] Legs;
        public Sprite[] Ticks;
        public Sprite[] Shadows;
        public float LegLength = 0.235f;
        public float LegInset = 0.005f;
        public float TickDrop = 0.045f;
        public RigHairBend HairBend;

        public float PixelsPerUnit => Eggs[0].pixelsPerUnit;
    }
}
