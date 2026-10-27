using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Weapon.Network;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class CursorAim
    {
        private const float MinAimDistance = 0.01f;
        private const float MinAimReach = 1.5f;

        private readonly CursorPoint _cursor;
        private readonly IReadonlyHands _hands;
        private readonly IAimDeflections _deflections;
        private readonly Transform _character;

        private Vector2 _toCursor = Vector2.right;

        public CursorAim(
            CursorPoint cursor,
            IReadonlyHands hands,
            IAimDeflections deflections,
            Transform character)
        {
            _cursor = cursor;
            _hands = hands;
            _deflections = deflections;
            _character = character;
        }

        public Vector2 Sample()
        {
            var center = (Vector2)_character.position;
            var toCursor = _cursor.World - center;
            if (toCursor.sqrMagnitude > MinAimDistance * MinAimDistance)
                _toCursor = toCursor;

            var direction = (Vector2)(Quaternion.Euler(0f, 0f, Deflection()) * _toCursor.normalized);
            return center + direction * Mathf.Max(_toCursor.magnitude, MinAimReach);
        }

        private float Deflection() =>
            _hands.TryFindInHands(out var inHands) ? _deflections.ByWeapon[inHands].Current : 0f;
    }
}
