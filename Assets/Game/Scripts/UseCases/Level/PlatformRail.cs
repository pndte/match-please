using UnityEngine;

namespace Bw.UseCases.Level
{
    public readonly struct PlatformRail
    {
        public readonly Vector2 Start;
        public readonly Vector2 Travel;

        public PlatformRail(Vector2 start, Vector2 travel)
        {
            Start = start;
            Travel = travel;
        }
    }
}
