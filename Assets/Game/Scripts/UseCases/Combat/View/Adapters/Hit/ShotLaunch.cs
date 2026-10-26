using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Adapters.Hit
{
    public readonly struct ShotLaunch
    {
        public readonly IReadonlyMortal Target;
        public readonly Vector2 From;
        public readonly float Speed;

        public ShotLaunch(IReadonlyMortal target, Vector2 from, float speed)
        {
            Target = target;
            From = from;
            Speed = speed;
        }
    }
}
