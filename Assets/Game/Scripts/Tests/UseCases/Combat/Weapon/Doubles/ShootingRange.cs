using System;
using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.UseCases.Combat;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network;
using JetBrains.Lifetimes;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class ShootingRange : IDisposable
    {
        public const int Ground = 14;
        public const int Characters = 15;
        public const int HeldProps = 17;
        public const int Corpses = 2;
        public const int Weapons = 13;

        public static readonly LayerMask Struck = (1 << Ground) | (1 << Characters) | (1 << HeldProps);

        public IPhysicsSpace Space => _space;
        public Lifetime Lifetime => _lifetime.Lifetime;

        private const string PhysicsConfigPath = "Assets/Game/Content/Physics/PhysicsConfig.asset";

        private readonly LifetimeDefinition _lifetime = new();
        private readonly HiddenObjects _objects = new();
        private readonly List<Object> _assets = new();
        private readonly PhysicsSpace _space;

        public ShootingRange()
        {
            var config = AssetDatabase.LoadAssetAtPath<PhysicsConfig>(PhysicsConfigPath);
            _space = PhysicsSpace.Create(_lifetime.Lifetime, config, new ManualClock());
        }

        public PhysicsBody Wall(Vector2 center, Vector2 size, bool oneWay = false) =>
            _space.CreateBody(_lifetime.Lifetime, Box("Wall", center, size, Ground, oneWay), PhysicsBody.BodyType.Static);

        public RangeTarget Person(Vector2 center) =>
            Target("Person", center, new Vector2(1f, 2f), Characters);

        public RangeTarget Target(
            string name,
            Vector2 center,
            Vector2 size,
            int layer)
        {
            var box = Box(name, center, size, layer, false);
            var body = _space.CreateBody(_lifetime.Lifetime, box, PhysicsBody.BodyType.Kinematic);
            var mortal = new FakeMortal();
            var container = new DiContainer();
            container.Bind<IReadonlyMortal>().FromInstance(mortal);
            container.Inject(box.gameObject.AddComponent<ReadonlyMortalHolder>());
            return new RangeTarget(mortal, body, box);
        }

        public PhysicsBody FloorWeapon(IWeapon weapon, Vector2 center, Vector2 size)
        {
            var box = Box("Weapon", center, size, Weapons, false);
            var container = new DiContainer();
            container.Bind<IWeapon>().FromInstance(weapon);
            container.Inject(box.gameObject.AddComponent<WeaponHolder>());
            return _space.CreateBody(_lifetime.Lifetime, box, PhysicsBody.BodyType.Kinematic);
        }

        public BodyBox Box(
            string name,
            Vector2 center,
            Vector2 size,
            int layer,
            bool oneWay) =>
            _objects.Create(name, center, layer).AddComponent<BodyBox>().With("_size", size).With("_oneWay", oneWay);

        public Transform Point(string name, Vector2 position) =>
            _objects.Create(name, position).transform;

        public T Config<T>() where T : ScriptableObject
        {
            var config = ScriptableObject.CreateInstance<T>();
            _assets.Add(config);
            return config;
        }

        public WeaponOrbit Orbit(float minRadius, float maxRadius, float detection = 1.5f)
        {
            var config = Config<WeaponRotationConfig>();
            config.MinOrbitRadius = minRadius;
            config.MaxOrbitRadius = maxRadius;
            config.ObstacleDetectionDistance = detection;
            config.ObstacleLayerMask = 1 << Ground;
            return new WeaponOrbit(config, _space);
        }

        public ShotTracer Gun(
            IWeapon weapon,
            IWeaponsInHands inHands,
            Vector2 muzzle,
            PelletsConfig pellets)
        {
            var config = Config<RaycastShootConfig>();
            config.MaxDistance = 20f;
            config.HitMask = Struck;
            var weaponMuzzle = new WeaponMuzzle(Point("Muzzle", muzzle), Orbit(0f, 0f));
            return new ShotTracer(weapon, inHands, weaponMuzzle, config, pellets, _space);
        }

        public void Dispose()
        {
            _lifetime.Terminate();
            _objects.Dispose();
            foreach (var asset in _assets)
                Object.DestroyImmediate(asset);

            _assets.Clear();
        }
    }
}
