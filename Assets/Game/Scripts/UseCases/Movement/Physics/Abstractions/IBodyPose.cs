using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IBodyPose
    {
        public void Place(Vector2 position, float angle);
    }
}
