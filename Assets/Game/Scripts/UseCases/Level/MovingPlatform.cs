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

        private readonly Rigidbody2D _body;
        private readonly Transform _transform;
        private readonly Vector2 _start;
        private readonly Rect _shape;
        private readonly MovingPlatformPath _path;
        private readonly ISimulationStep _step;

        public MovingPlatform(Rigidbody2D body, BoxCollider2D collider, MovingPlatformPath path, ISimulationStep step)
        {
            _body = body;
            _transform = body.transform;
            _start = _transform.position;
            _shape = ShapeOf(collider, _transform.lossyScale);
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

        private void Place(Vector2 position) //TODO: брошенное оружие (динамическое тело) платформа не везёт: она переставляется раз в тик без скорости, трение паллеты пушку не тянет, а лифт выталкивает её рывками — двигать тело для физики через MovePosition со скоростью
        {
            _body.position = position;
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
        }

        private static Rect ShapeOf(BoxCollider2D collider, Vector3 scale)
        {
            var size = Vector2.Scale(collider.size, scale);
            return new Rect(Vector2.Scale(collider.offset, scale) - size / 2f, size);
        }
    }
}
