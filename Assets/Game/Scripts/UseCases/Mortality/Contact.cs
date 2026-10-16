using UnityEngine;

namespace Bw.UseCases.Mortality
{
    public readonly struct Contact
    {
        public readonly Vector2 Point;

        public Contact(Vector2 point)
        {
            Point = point;
        }
    }
}
