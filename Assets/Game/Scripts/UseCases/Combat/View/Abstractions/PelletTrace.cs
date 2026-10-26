using UnityEngine;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public readonly struct PelletTrace
    {
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly Vector2 Direction;
        public readonly PelletEnd End;
        public readonly Vector2 Normal;
        public readonly int Layer;

        public PelletTrace(
            Vector2 from,
            Vector2 to,
            Vector2 direction,
            PelletEnd end,
            Vector2 normal,
            int layer)
        {
            From = from;
            To = to;
            Direction = direction;
            End = end;
            Normal = normal;
            Layer = layer;
        }
    }
}
