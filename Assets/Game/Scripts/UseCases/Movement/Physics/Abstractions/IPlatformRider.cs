using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IPlatformRider
    {
        public Vector2 RideShift(Vector2 position, double fromSeconds, double toSeconds);
    }
}
