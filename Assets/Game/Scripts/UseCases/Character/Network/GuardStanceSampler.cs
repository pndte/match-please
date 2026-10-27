using Bw.Entities;
using Bw.Entities.Physics;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Mortality.Guarding;
using UnityEngine;

namespace Bw.UseCases.Character.Network
{
    public sealed class GuardStanceSampler //TODO: угол стойки считается от нарисованного тела, а блок ставится у тела в симуляции: на бегу и в прыжке он уходит от курсора на несколько градусов, как раньше уходил выстрел
    {
        private readonly GuardKey _key;
        private readonly CursorPoint _cursor;
        private readonly BodyBox _box;

        public GuardStanceSampler(GuardKey key, CursorPoint cursor, BodyBox box)
        {
            _key = key;
            _cursor = cursor;
            _box = box;
        }

        public GuardStance Sample()
        {
            if (!_key.Held.Value)
                return GuardStance.Lowered;

            var toCursor = _cursor.World - ((Vector2)_box.transform.position + _box.Offset);
            return new GuardStance(true, Mathf.Atan2(toCursor.y, toCursor.x) * Mathf.Rad2Deg);
        }
    }
}
