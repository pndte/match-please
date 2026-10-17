using System;
using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    [Serializable]
    public sealed class RigFoot
    {
        public SpriteRenderer Shoe;
        public SpriteRenderer Leg;
        public SpriteRenderer Tick;
        public Transform Hip;
        public Transform Ankle;
    }
}
