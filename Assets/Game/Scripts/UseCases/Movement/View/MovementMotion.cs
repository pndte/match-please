using UnityEngine;

namespace Bw.UseCases.Movement.View
{
    public readonly struct MovementMotion
    {
        public readonly Vector2 Position;
        public readonly float Run;
        public readonly float Rise;
        public readonly bool Grounded;

        public MovementMotion(
            Vector2 position,
            float run,
            float rise,
            bool grounded)
        {
            Position = position;
            Run = run;
            Rise = rise;
            Grounded = grounded;
        }
    }
}
