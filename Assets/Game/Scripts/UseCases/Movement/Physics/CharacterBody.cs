using System.Collections.Generic;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class CharacterBody : IMovementBody, IMovementCollider
    {
        private const float Skin = 0.015f;

        public Vector2 Position => _transform.position;

        private readonly Rigidbody2D _rigidbody;
        private readonly Transform _transform;
        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly ContactFilter2D _filter;
        private readonly List<RaycastHit2D> _hits = new(4);

        public CharacterBody(Rigidbody2D rigidbody, BoxCollider2D collider, MovementConfig config)
        {
            _rigidbody = rigidbody;
            _transform = rigidbody.transform;
            _size = Vector2.Scale(collider.size, _transform.lossyScale);
            _offset = Vector2.Scale(collider.offset, _transform.lossyScale);
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.GroundLayer,
                useTriggers = false
            };
        }

        public void Place(Vector2 position)
        {
            _rigidbody.position = position;
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
        }

        public float Cast(Vector2 position, Vector2 direction, float distance)
        {
            var hitCount = Physics2D.BoxCast(position + _offset, _size, 0f, direction, _filter, _hits, distance + Skin);

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hits[i];
                if (Vector2.Dot(hit.normal, direction) < 0f)
                    distance = Mathf.Min(distance, hit.distance - Skin);
            }

            return Mathf.Max(0f, distance);
        }
    }
}
