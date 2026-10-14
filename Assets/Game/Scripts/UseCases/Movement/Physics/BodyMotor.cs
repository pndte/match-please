using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class BodyMotor : IBodyMotor
    {
        private const int MaxShoves = 8;
        private const float Touching = 0.001f;

        private readonly IMovementCollider _collider;
        private readonly ISimulationStep _step;
        private readonly IBodyConfig _config;
        private readonly float _gravity;

        public BodyMotor(IMovementCollider collider, ISimulationStep step, IBodyConfig config)
        {
            _collider = collider;
            _step = step;
            _config = config;
            _gravity = Physics2D.gravity.y * config.GravityScale;
        }

        public BodyState Step(BodyState state, BodyDrive drive, out bool launched)
        {
            var deltaTime = _step.Duration;
            var position = _collider.Carry(state.Position, state.Grounded);
            var launchHeight = 0f;
            launched = state.Grounded && _collider.TryGetLaunch(position, out launchHeight);

            var velocity = launched ? drive.Velocity.WithY(LaunchSpeed(launchHeight)) : drive.Velocity;
            var gravityScale = launched ? 1f : drive.GravityScale;
            velocity = velocity.WithY(velocity.y + _gravity * gravityScale * deltaTime);

            var push = state.HorizontalPush;
            var horizontal = velocity.x + push;
            if (horizontal != 0f)
            {
                var direction = Mathf.Sign(horizontal);
                var distance = Mathf.Abs(horizontal) * deltaTime;
                var way = new Vector2(direction, 0f);
                var travel = HorizontalTravel(position, way, distance, drive);
                if (Walking(way, drive))
                    MakeWay(position, way, travel, _collider);

                position += way * travel;
                if (travel < distance && Mathf.Sign(push) == direction)
                    push = 0f;
            }

            push = _config.DeceleratedPush(push, state.Grounded, _step);

            var verticalDirection = velocity.y > 0f ? Vector2.up : Vector2.down;
            var verticalDistance = Mathf.Abs(velocity.y) * deltaTime;
            var travelled = verticalDirection == Vector2.down && !drive.Dropping
                ? Fall(ref position, verticalDistance)
                : Sweep(ref position, verticalDirection, verticalDistance);

            if (travelled >= verticalDistance)
                return new BodyState(position, velocity, push, false);

            return new BodyState(position, velocity.WithY(0f), push, verticalDirection == Vector2.down);
        }

        public float PossibleShove(
            Vector2 position,
            Vector2 direction,
            float distance,
            IMovementCollider shover)
        {
            var possible = PossibleAdvance(position, direction, distance, float.PositiveInfinity, shover);
            return possible < Touching ? 0f : possible;
        }

        public void MakeWay(
            Vector2 position,
            Vector2 direction,
            float distance,
            IMovementCollider shover)
        {
            for (var shoves = 0; TryGetNearest(_collider.ShovablesAhead(position, direction, distance, shover), distance, out var nearest); shoves++)
            {
                if (shoves == MaxShoves)
                    throw new InvalidOperationException(
                        $"Making way of {distance.ToString()} took more than {MaxShoves.ToString()} shoves: the possible advance promised a way its shoves did not clear.");

                nearest.Shovable.Shove(direction, distance - nearest.Gap, shover);
            }
        }

        private float LaunchSpeed(float height) =>
            -_gravity * _step.Duration / 2f + Mathf.Sqrt(-2f * _gravity * height);

        private float HorizontalTravel(
            Vector2 position,
            Vector2 direction,
            float distance,
            BodyDrive drive) =>
            Walking(direction, drive)
                ? PossibleAdvance(position, direction, distance, _config.ShoveSpeed * _step.Duration, _collider)
                : _collider.Cast(position, direction, distance);

        private float PossibleAdvance(
            Vector2 position,
            Vector2 direction,
            float distance,
            float reach,
            IMovementCollider shover)
        {
            var free = _collider.WallDistance(position, direction, distance, shover);
            var contacts = _collider.ShovablesAhead(position, direction, free, shover);
            if (!TryGetNearest(contacts, free, out var nearest))
                return free;

            if (CountInWay(contacts, free) > MaxShoves)
                return Mathf.Max(0f, nearest.Gap);

            var possible = Mathf.Min(free, Mathf.Max(nearest.Gap, 0f) + reach);
            for (var index = 0; index < contacts.Count; index++)
            {
                var contact = contacts[index];
                if (contact.Gap < possible - Touching)
                    possible = Mathf.Min(possible, contact.Gap + contact.Shovable.PossibleShove(direction, possible - contact.Gap, shover));
            }

            return Mathf.Max(0f, possible);
        }

        private static bool Walking(Vector2 direction, BodyDrive drive) =>
            drive.Velocity.x * direction.x > 0f;

        private static bool TryGetNearest(IReadOnlyList<ShovableContact> contacts, float distance, out ShovableContact nearest)
        {
            var found = false;
            nearest = default;
            for (var index = 0; index < contacts.Count; index++)
            {
                var contact = contacts[index];
                if (contact.Gap >= distance - Touching || found && contact.Gap >= nearest.Gap)
                    continue;

                nearest = contact;
                found = true;
            }

            return found;
        }

        private static int CountInWay(IReadOnlyList<ShovableContact> contacts, float distance)
        {
            var count = 0;
            for (var index = 0; index < contacts.Count; index++)
                if (contacts[index].Gap < distance - Touching)
                    count++;

            return count;
        }

        private float Sweep(ref Vector2 position, Vector2 direction, float distance)
        {
            var allowed = _collider.Cast(position, direction, distance);
            position += direction * allowed;
            return allowed;
        }

        private float Fall(ref Vector2 position, float distance)
        {
            var allowed = _collider.Fall(position, distance);
            position += Vector2.down * allowed;
            return allowed;
        }
    }
}
