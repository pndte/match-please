using System;
using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    [Serializable]
    public sealed class CharacterRigParts
    {
        public Transform Body;
        public SpriteRenderer Egg;
        public SpriteRenderer Face;
        public SpriteRenderer Hair;
        public SpriteRenderer Shadow;
        public SpriteRenderer Fx;
        public RigFoot Far;
        public RigFoot Near;

        public Transform[] Posed => new[]
        {
            Body,
            Shadow.transform,
            Far.Shoe.transform,
            Far.Leg.transform,
            Far.Tick.transform,
            Near.Shoe.transform,
            Near.Leg.transform,
            Near.Tick.transform
        };

        public SpriteRenderer[] Drawn => new[] { Egg, Face, Hair, Shadow, Fx, Far.Shoe, Far.Leg, Far.Tick, Near.Shoe, Near.Leg, Near.Tick };
    }
}
