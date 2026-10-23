using Bw.UseCases.Combat.Abstractions.Weapons;
using UnityEngine;

namespace Bw.UseCases.Bots.Senses
{
    public readonly struct PropSighting
    {
        public readonly IWeapon Prop;
        public readonly Vector2 Position;

        public PropSighting(IWeapon prop, Vector2 position)
        {
            Prop = prop;
            Position = position;
        }
    }
}
