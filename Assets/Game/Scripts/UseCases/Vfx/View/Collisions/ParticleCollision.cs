using System;
using JetBrains.Lifetimes;
using R3;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Vfx.View.Collisions
{
    public sealed class ParticleCollision //TODO: бенчмарки
    {
        private const float Skin = 0.002f;
        private const float OverlapPush = 0.2f;
        private const float OverlapClearance = 0.005f;
        private const float SpentLifetime = 1e-6f;
        private const uint BounceSalt = 0x5BD1E995u;
        private const uint DampenSalt = 0x68E31DA4u;
        private const uint LifetimeLossSalt = 0xB5297A4Du;

        private readonly ParticleSystem _particles;
        private readonly ParticleCollisionConfig _config;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _surfaces;
        private readonly ParticleSystem.Particle[] _buffer;

        public ParticleCollision(
            Lifetime lifetime,
            ParticleSystem particles,
            ParticleCollisionConfig config,
            PhysicsWorld world)
        {
            _particles = particles;
            _config = config;
            _world = world;
            _surfaces = new PhysicsQuery.QueryFilter(PhysicsMask.All, new PhysicsMask(config.Surfaces));
            _buffer = new ParticleSystem.Particle[particles.main.maxParticles];

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, lifetime).Subscribe(_ => Collide(DeltaTime()));
        }

        public void Collide(float deltaTime)
        {
            if (_particles.particleCount == 0)
                return;

            var count = _particles.GetParticles(_buffer);
            var toWorld = SpaceToWorld();
            var toSpace = toWorld.inverse;
            var struck = false;
            for (var index = 0; index < count; index++)
            {
                if (!TryBounce(ref _buffer[index], toWorld, toSpace, deltaTime))
                    continue;

                TriggerSubEmitters(ref _buffer[index]);
                struck = true;
            }

            if (struck)
                _particles.SetParticles(_buffer, count);
        }

        private bool TryBounce(
            ref ParticleSystem.Particle particle,
            Matrix4x4 toWorld,
            Matrix4x4 toSpace,
            float deltaTime)
        {
            var end = toWorld.MultiplyPoint3x4(particle.position);
            var velocity = (Vector2)toWorld.MultiplyVector(particle.totalVelocity);
            var elapsed = Mathf.Min(deltaTime, particle.startLifetime - particle.remainingLifetime);
            var translation = velocity * elapsed;
            var circle = new CircleGeometry { center = (Vector2)end - translation, radius = RadiusBefore(particle, elapsed) };
            if (!TryFindContact(circle, translation, out var contact))
                return false;

            var response = ResponseOf(particle);
            var bounced = response.Bounced(velocity, contact.Normal);
            var position = contact.Center + contact.Normal * response.PushOf(contact.Depth) + bounced * ((1f - contact.Fraction) * elapsed);
            var animated = particle.totalVelocity - particle.velocity;
            particle.position = toSpace.MultiplyPoint3x4(new Vector3(position.x, position.y, end.z));
            particle.velocity = toSpace.MultiplyVector(bounced) - animated;
            particle.remainingLifetime = Mathf.Max(particle.remainingLifetime - response.LifetimeLoss * particle.startLifetime, SpentLifetime);
            return true;
        }

        private bool TryFindContact(CircleGeometry circle, Vector2 translation, out Contact contact)
        {
            using var hits = _world.CastGeometry(circle, translation, _surfaces, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            foreach (var hit in hits)
            {
                if (hit.normal == Vector2.zero)
                {
                    contact = OverlapContact(hit.shape, circle);
                    return true;
                }

                if (Vector2.Dot(translation, hit.normal) >= 0f)
                    continue;

                contact = new Contact(circle.center + translation * hit.fraction + hit.normal * Skin, hit.normal, hit.fraction, 0f);
                return true;
            }

            contact = default;
            return false;
        }

        private float RadiusBefore(ParticleSystem.Particle particle, float elapsed)
        {
            particle.remainingLifetime += elapsed;
            return particle.GetCurrentSize(_particles) * _config.RadiusScale * 0.5f;
        }

        private Response ResponseOf(in ParticleSystem.Particle particle)
        {
            var age = 1f - particle.remainingLifetime / particle.startLifetime;
            return new Response(
                _config.Bounce.Evaluate(age, RandomOf(particle.randomSeed, BounceSalt)),
                _config.Dampen.Evaluate(age, RandomOf(particle.randomSeed, DampenSalt)),
                _config.LifetimeLoss.Evaluate(age, RandomOf(particle.randomSeed, LifetimeLossSalt)));
        }

        private void TriggerSubEmitters(ref ParticleSystem.Particle particle)
        {
            var subEmitters = _particles.subEmitters;
            if (!subEmitters.enabled)
                return;

            for (var index = 0; index < subEmitters.subEmittersCount; index++)
                if (subEmitters.GetSubEmitterType(index) == ParticleSystemSubEmitterType.Manual)
                    _particles.TriggerSubEmitter(index, ref particle);
        }

        private Matrix4x4 SpaceToWorld() =>
            _particles.main.simulationSpace == ParticleSystemSimulationSpace.Local ? _particles.transform.localToWorldMatrix : Matrix4x4.identity;

        private float DeltaTime()
        {
            var main = _particles.main;
            return (main.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * main.simulationSpeed;
        }

        private static Contact OverlapContact(PhysicsShape surface, CircleGeometry circle)
        {
            var manifold = ManifoldOf(surface, circle);
            return new Contact(circle.center, manifold.normal, 0f, OverlapClearance - manifold[0].separation);
        }

        private static PhysicsShape.ContactManifold ManifoldOf(PhysicsShape surface, CircleGeometry circle) =>
            surface.shapeType switch
            {
                PhysicsShape.ShapeType.Polygon => surface.polygonGeometry.Intersect(surface.transform, circle, PhysicsTransform.identity),
                PhysicsShape.ShapeType.Circle => surface.circleGeometry.Intersect(surface.transform, circle, PhysicsTransform.identity),
                PhysicsShape.ShapeType.Capsule => surface.capsuleGeometry.Intersect(surface.transform, circle, PhysicsTransform.identity),
                PhysicsShape.ShapeType.Segment => surface.segmentGeometry.Intersect(surface.transform, circle, PhysicsTransform.identity),
                _ => throw new InvalidOperationException($"A particle cannot be pushed out of a {surface.shapeType} surface: LowLevelPhysics2D gives no manifold against it.")
            };

        private static float RandomOf(uint seed, uint salt)
        {
            var hash = seed ^ salt;
            hash ^= hash >> 16;
            hash *= 0x7FEB352Du;
            hash ^= hash >> 15;
            hash *= 0x846CA68Bu;
            hash ^= hash >> 16;
            return (hash >> 8) / 16777216f;
        }

        private readonly struct Contact
        {
            public readonly Vector2 Center;
            public readonly Vector2 Normal;
            public readonly float Fraction;
            public readonly float Depth;

            public Contact(
                Vector2 center,
                Vector2 normal,
                float fraction,
                float depth)
            {
                Center = center;
                Normal = normal;
                Fraction = fraction;
                Depth = depth;
            }
        }

        private readonly struct Response
        {
            public readonly float Bounce;
            public readonly float Dampen;
            public readonly float LifetimeLoss;

            public Response(float bounce, float dampen, float lifetimeLoss)
            {
                Bounce = bounce;
                Dampen = dampen;
                LifetimeLoss = lifetimeLoss;
            }

            public Vector2 Bounced(Vector2 velocity, Vector2 normal) =>
                (velocity - normal * (Vector2.Dot(velocity, normal) * (1f + Bounce))) * (1f - Dampen);

            public float PushOf(float depth) =>
                depth * OverlapPush * (1f + Bounce * (1f - Dampen));
        }
    }
}
