using UnityEngine;

namespace Bw.EditorTools.Shooting
{
    public readonly struct PreviewPart
    {
        public readonly Sprite Sprite;
        public readonly Matrix4x4 ToVisual;

        public PreviewPart(Sprite sprite, Matrix4x4 toVisual)
        {
            Sprite = sprite;
            ToVisual = toVisual;
        }
    }
}
