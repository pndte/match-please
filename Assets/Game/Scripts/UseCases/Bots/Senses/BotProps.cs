using Bw.Entities;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Movement.Abstractions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Bots.Senses
{
    public sealed class BotProps
    {
        private Vector2 Position => _movement.State.Value.Body.Position;

        private readonly IReadonlyMovement _movement;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _props;
        private readonly IReadonlyWeaponKindRegistry _kinds;
        private readonly IReadonlyWeaponCarriers _carriers;
        private readonly WeaponPickupConfig _pickup;
        private readonly BotsConfig _bots;

        public BotProps(
            IReadonlyMovement movement,
            IPhysicsSpace space,
            IReadonlyWeaponKindRegistry kinds,
            IReadonlyWeaponCarriers carriers,
            WeaponPickupConfig pickup,
            BotsConfig bots)
        {
            _movement = movement;
            _world = space.World;
            _props = new PhysicsQuery.QueryFilter(PhysicsMask.All, pickup.WeaponMask.AsPhysicsMask());
            _kinds = kinds;
            _carriers = carriers;
            _pickup = pickup;
            _bots = bots;
        }

        public bool TryFindNearest(out PropSighting nearest)
        {
            var position = Position;
            var closest = float.MaxValue;
            nearest = default;
            using var found = _world.OverlapGeometry(new CircleGeometry { center = position, radius = _bots.PropSearchRange }, _props, Allocator.Temp);
            for (var index = 0; index < found.Length; index++)
            {
                var shape = found[index].shape;
                if (!shape.TryGetComponent<IHolder<IWeapon>>(out var holder) || _carriers.ByWeapon.ContainsKey(holder.Value) || _kinds.KindOf(holder.Value) != WeaponKind.Prop)
                    continue;

                var center = shape.aabb.center;
                var distance = (center - position).sqrMagnitude;
                if (distance >= closest)
                    continue;

                closest = distance;
                nearest = new PropSighting(holder.Value, center);
            }

            return closest < float.MaxValue;
        }

        public bool IsPickable(PropSighting prop) =>
            (prop.Position - Position).sqrMagnitude <= _pickup.Reach * _pickup.Reach;
    }
}
