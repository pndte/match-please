using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Weapon.Network;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class CursorAim
    {
        private const float MinAimDistance = 0.01f;

        private readonly CursorPoint _cursor;
        private readonly IReadonlyHands _hands;
        private readonly IAimDeflections _deflections;
        private readonly Transform _character;

        private float _aim;

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

        public float Sample()
        {
            var target = (Vector2)_character.InverseTransformPoint(_cursor.World);
            if (target.sqrMagnitude > MinAimDistance * MinAimDistance)
                _aim = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;

            return Mathf.DeltaAngle(0f, _aim + Deflection());
        }

        private float Deflection() =>
            _hands.TryFindInHands(out var inHands) ? _deflections.ByWeapon[inHands].Current : 0f;
    }
}
