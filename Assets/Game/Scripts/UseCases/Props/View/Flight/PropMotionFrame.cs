using UnityEngine;

namespace Bw.UseCases.Props.View.Flight
{
    public readonly struct PropMotionFrame
    {
        public readonly Vector2 Center;
        public readonly Vector2 Velocity;
        public readonly bool Held;
        public readonly bool Fast;

        public PropMotionFrame(
            Vector2 center,
            Vector2 velocity,
            bool held,
            bool fast)
        {
            Center = center;
            Velocity = velocity;
            Held = held;
            Fast = fast;
        }
    }
}
