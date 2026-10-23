using UnityEngine;

namespace Bw.UseCases.Props.View.Abstractions
{
    public interface IHeldPropViewModel
    {
        public Vector2 Position { get; }
        public float Angle { get; }
    }
}
