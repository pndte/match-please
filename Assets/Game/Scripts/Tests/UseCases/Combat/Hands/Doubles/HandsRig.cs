using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class HandsRig
    {
        public FakeMortal Mortal { get; } = new();
        public FakeMovement Movement { get; } = new();
        public CharacterInventory Inventory { get; }
        public HandsSimulator Rules { get; }
        public HandsGrip Grip { get; }
        public PhysicsBody Body { get; }
        public Bw.UseCases.Combat.Hands.Hands Hands { get; }
        public DrawnWeapons Draw { get; }
        public Bw.UseCases.Combat.Characters.Character Character { get; }

        public HandsRig(
            Lifetime lifetime,
            InventoryRig inventories,
            HoldOval oval,
            bool guardedByBusyHands = false,
            IPhysicsSpace space = null)
        {
            Inventory = inventories.InventoryOf(Mortal);
            Rules = new HandsSimulator(Inventory);
            Grip = new HandsGrip(space ?? new NoPhysicsSpace());
            Body = BodyIn(lifetime, space, oval.transform.position);
            ISimulator<HandsInput, HandsState> simulator = guardedByBusyHands ? new BusyHandsGuard(Rules, Inventory) : Rules;
            Hands = new Bw.UseCases.Combat.Hands.Hands(simulator, Rules, Inventory, oval, Grip, Body);
            Draw = new DrawnWeapons(lifetime, Inventory, Hands);
            Character = new Bw.UseCases.Combat.Characters.Character(Mortal, Movement, Hands, Inventory);
        }

        public void Step(DrawChoice choice, bool guarded = false, bool pulled = false) =>
            Hands.Step(new HandsInput(new WeaponInput(0f, pulled, false, guarded), choice));

        public void MoveTo(Vector2 position)
        {
            var body = Body;
            body.position = position;
        }

        private static PhysicsBody BodyIn(Lifetime lifetime, IPhysicsSpace space, Vector2 position)
        {
            var world = space is NoPhysicsSpace or null ? OwnWorld(lifetime) : space.World;
            return world.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic, position = position });
        }

        private static PhysicsWorld OwnWorld(Lifetime lifetime)
        {
            var world = PhysicsWorld.Create();
            lifetime.OnTermination(() => world.Destroy());
            return world;
        }
    }
}
