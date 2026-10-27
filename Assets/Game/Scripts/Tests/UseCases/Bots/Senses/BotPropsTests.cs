using Bw.Entities.Physics.Extensions;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Pickup;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.Tests.UseCases.Bots.Senses
{
    public sealed class BotPropsTests
    {
        private const string PickupConfigPath = "Assets/Game/Content/Weapons/WeaponPickupConfig.asset";

        private PhysicsTestWorld _world;
        private BotsConfig _bots;
        private FakeWeapon _prop;
        private PhysicsBody _crate;
        private BotProps _props;

        [SetUp]
        public void SetUp()
        {
            _world = new PhysicsTestWorld();
            _bots = ScriptableObject.CreateInstance<BotsConfig>();
            var kinds = new WeaponKindRegistry();
            var movement = new FakeMovement();
            movement.MoveTo(Vector2.zero);
            _prop = new FakeWeapon();
            kinds.AddLifetimed(_world.Lifetime, _prop, WeaponKind.Prop);
            _crate = Crate(_prop, new Vector2(3f, 0.5f));
            _props = new BotProps(movement, _world.Space, kinds, new WeaponCarriers(), AssetDatabase.LoadAssetAtPath<WeaponPickupConfig>(PickupConfigPath), _bots);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_bots);
            _world.Dispose();
        }

        [Test]
        public void RestingPropIsFound()
        {
            Assert.That(_props.TryFindNearest(out var nearest), Is.True);
            Assert.That(nearest.Prop, Is.SameAs(_prop));
        }

        [Test]
        public void PropBarelyMovingIsFound()
        {
            _crate.linearVelocity = new Vector2(_bots.RestingPropSpeed * 0.5f, 0f);

            Assert.That(_props.TryFindNearest(out _), Is.True);
        }

        [Test]
        public void FlyingPropIsNotFetched()
        {
            _crate.linearVelocity = new Vector2(8f, 2f);

            Assert.That(_props.TryFindNearest(out _), Is.False, "a bot that fetched the crate it had just thrown caught it again at the start of its flight, over and over");
        }

        private PhysicsBody Crate(IWeapon prop, Vector2 position)
        {
            var box = _world.Box("Crate", position, Vector2.one);
            var container = new DiContainer();
            container.Bind<IWeapon>().FromInstance(prop);
            container.InstantiateComponent<WeaponHolder>(box.gameObject);
            return _world.Space.CreateBody(_world.Lifetime, box, PhysicsBody.BodyType.Dynamic);
        }
    }
}
