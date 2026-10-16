using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Level.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Level
{
    public sealed class MovingPlatform : IMovingPlatform
    {
        public Rect Area { get; private set; }
        public Vector2 Shift { get; private set; }

        private readonly SweptBody _body;
        private readonly Transform _transform;
        private readonly Vector2 _start;
        private readonly Rect _shape;
        private readonly MovingPlatformPath _path;
        private readonly ISimulationStep _step;

        public MovingPlatform(
            BodyBox box,
            SweptBody body,
            MovingPlatformPath path,
            ISimulationStep step)
        {
            _body = body;
            _transform = box.transform;
            _start = _transform.position;
            _shape = ShapeOf(box);
            _path = path;
            _step = step;
            Area = AreaAround(_start);
        }

        public void PoseAt(double seconds)
        {
            var position = PositionAt(seconds);
            Shift = position - PositionAt(seconds - _step.Duration);
            Area = AreaAround(position);
            Place(position);
        }

        public void ShowAt(double seconds) =>
            Place(PositionAt(seconds));

        public Rect AreaAt(double seconds) =>
            AreaAround(PositionAt(seconds));

        private Vector2 PositionAt(double seconds) =>
            _start + _path.OffsetAt(seconds);

        private Rect AreaAround(Vector2 position) =>
            new(position + _shape.position, _shape.size);

        private void Place(Vector2 position)
        {
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
            _body.Place(position);
        }

        private static Rect ShapeOf(BodyBox box)
        {
            var size = box.WorldSize();
            return new Rect(box.WorldOffset() - size / 2f, size);
        }
    }
}
