using Bw.Entities.Physics;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class BodyPicture : IBodyPicture
    {
        public Vector2 Position => _transform.position;

        private readonly Transform _transform;

        public BodyPicture(BodyBox box)
        {
            _transform = box.transform;
        }

        public void Place(Vector2 position) =>
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
    }
}
