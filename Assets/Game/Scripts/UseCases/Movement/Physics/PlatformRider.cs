using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class PlatformRider
    {
        private const float RideTolerance = 0.05f;

        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly IMovingPlatformCollection _platforms;

        public PlatformRider(BodyBox box, IMovingPlatformCollection platforms)
        {
            _size = box.WorldSize();
            _offset = box.WorldOffset();
            _platforms = platforms;
        }

        public bool TryGetRidden(Vector2 position, out Vector2 shift)
        {
            var bounds = UprightBounds(position);
            var top = float.NegativeInfinity;
            var found = false;
            shift = Vector2.zero;

            for (var index = 0; index < _platforms.Count; index++)
            {
                var platform = _platforms[index];
                var previous = new Rect(platform.Area.position - platform.Shift, platform.Area.size);
                if (!Riding(previous, bounds) || previous.yMax <= top)
                    continue;

                top = previous.yMax;
                shift = platform.Shift;
                found = true;
            }

            return found;
        }

        public Vector2 Scooped(Vector2 position, float skin) //TODO: подхват поднимающейся платформой не знает про S — со сквозной доски, которая едет вверх, на медленном ходу не спрыгнуть: она подхватывает снова; потолок над платформой подхват не проверяет, поэтому лифт может вдавить ящик в стоящего над ним персонажа, а влезших друг в друга мотор не расталкивает: оба упираются друг в друга, пока ящик не разобьют
        {
            var bounds = UprightBounds(position);
            var feet = bounds.yMin;
            var lift = 0f;

            for (var index = 0; index < _platforms.Count; index++)
            {
                var platform = _platforms[index];
                var area = platform.Area;
                var previousTop = area.yMax - platform.Shift.y;
                if (platform.Shift.y > 0f && Overlapping(area, bounds) && feet >= previousTop - RideTolerance && feet < area.yMax + skin)
                    lift = Mathf.Max(lift, area.yMax + skin - feet);
            }

            return position + Vector2.up * lift;
        }

        private Rect UprightBounds(Vector2 position) =>
            new(position + _offset - _size / 2f, _size);

        private static bool Riding(Rect area, Rect bounds) =>
            Overlapping(area, bounds) && Mathf.Abs(bounds.yMin - area.yMax) <= RideTolerance;

        private static bool Overlapping(Rect area, Rect bounds) =>
            bounds.xMin < area.xMax && bounds.xMax > area.xMin;
    }
}
