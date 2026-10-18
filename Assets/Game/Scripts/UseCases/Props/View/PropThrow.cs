using UnityEngine;

namespace Bw.UseCases.Props.View
{
    public readonly struct PropThrow
    {
        public readonly Vector2 Direction;

        public PropThrow(Vector2 direction)
        {
            Direction = direction;
        }
    }
}
