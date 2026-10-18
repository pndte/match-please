using System;
using System.Linq;
using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    [Serializable]
    public sealed class CharacterRigParts
    {
        public Transform Body;
        public SpriteRenderer Egg;
        public SpriteRenderer Skin;
        public SpriteRenderer Face;
        public SpriteRenderer Hair;
        public SpriteRenderer Back;
        public SpriteRenderer Over;
        public SpriteRenderer Shadow;
        public SpriteRenderer Fx;
        public RigFoot Far;
        public RigFoot Near;
        public SpriteRenderer[] Flaps;

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
            }
            .Concat(Flaps.Select(flap => flap.transform))
            .ToArray();

        public SpriteRenderer[] Drawn => new[] { Egg, Skin, Face, Hair, Back, Over, Shadow, Fx, Far.Shoe, Far.Leg, Far.Tick, Near.Shoe, Near.Leg, Near.Tick }
            .Concat(Flaps)
            .ToArray();
    }
}
