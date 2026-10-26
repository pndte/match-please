using System;
using Bw.Entities.Extensions;
using Bw.Entities.Physics;
using Bw.UseCases.Vfx.View.Collisions;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Vfx
{
    public class ParticleCollisionInstaller : MonoInstaller
    {
        [SerializeField] private CollidingParticles[] _systems;

        public override void InstallBindings()
        {
            foreach (var system in _systems)
            {
                RequireCollidable(system);
                Container.Bind<ParticleCollisionSolver>()
                    .FromMethod(context => new ParticleCollisionSolver(gameObject.Lifetime(), system.Particles, system.Config, context.Container.Resolve<IPhysicsSpace>().World))
                    .AsCached()
                    .NonLazy();
            }
        }

        private void RequireCollidable(CollidingParticles system)
        {
            if (system.Particles == null || system.Config == null)
                throw new InvalidOperationException($"Effect '{name}' lists a colliding particle system without its system or its collision config.");

            if (system.Particles.collision.enabled)
                throw new InvalidOperationException($"Particle system '{system.Particles.name}' of '{name}' still collides by Unity's own module: the Box2D pass would collide it twice.");

            if (system.Particles.main.simulationSpace == ParticleSystemSimulationSpace.Custom)
                throw new InvalidOperationException($"Particle system '{system.Particles.name}' of '{name}' simulates in a custom space: the Box2D pass knows only the local and the world space.");

            RequireBetween(system.Config.Bounce, 0f, float.PositiveInfinity, nameof(ParticleCollisionConfig.Bounce), system.Config);
            RequireBetween(system.Config.Dampen, 0f, 1f, nameof(ParticleCollisionConfig.Dampen), system.Config);
            RequireBetween(system.Config.LifetimeLoss, 0f, 1f, nameof(ParticleCollisionConfig.LifetimeLoss), system.Config);
        }

        private static void RequireBetween(
            ParticleSystem.MinMaxCurve value,
            float min,
            float max,
            string property,
            ParticleCollisionConfig config)
        {
            var (first, second) = value.mode switch
            {
                ParticleSystemCurveMode.Constant => (value.constant, value.constant),
                ParticleSystemCurveMode.TwoConstants => (value.constantMin, value.constantMax),
                _ => throw new InvalidOperationException($"{property} of '{config.name}' is a {value.mode}: the Box2D pass takes a constant or a random value between two constants.")
            };

            if (Mathf.Min(first, second) < min || Mathf.Max(first, second) > max)
                throw new InvalidOperationException($"{property} of '{config.name}' runs {first}..{second}, outside {min}..{max}.");
        }
    }
}
