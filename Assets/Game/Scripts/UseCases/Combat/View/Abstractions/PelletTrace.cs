using UnityEngine;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public readonly struct PelletTrace
    {
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly Vector2 Direction;
        public readonly bool Struck;
        public readonly Vector2 Normal;
        public readonly int Layer;

        public PelletTrace(
            Vector2 from,
            Vector2 to,
            Vector2 direction,
            bool struck,
            Vector2 normal,
            int layer)
        {
            From = from;
            To = to;
            Direction = direction;
            Struck = struck;
            Normal = normal;
            Layer = layer;
        }
    }
}
