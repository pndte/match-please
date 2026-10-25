using System;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    public sealed class PhysicsSpace : IPhysicsSpace, IPhysicsSteps, ISteppable
    {
        public PhysicsWorld World { get; }
        public ISource<Unit> Stepped => _stepped;

        private readonly ISimulationStep _step;
        private readonly OneWayContacts _oneWay = new();
        private readonly Signal<Unit> _stepped = new();

        private PhysicsSpace(PhysicsWorld world, ISimulationStep step)
        {
            World = world;
            _step = step;
        }

        public static PhysicsSpace Create(Lifetime lifetime, PhysicsConfig config, ISimulationStep step)
        {
            var definition = PhysicsWorldDefinition.defaultDefinition;
            definition.simulateType = PhysicsWorld.SimulationType.Script;
            definition.gravity = Physics2D.gravity;
            definition.simulationSubSteps = config.SubSteps;
            definition.simulationWorkers = config.Workers;
            definition.sleepingAllowed = config.Sleeping;
            definition.contactFrequency = config.ContactFrequency;
            definition.contactDamping = config.ContactDamping;
            definition.contactSpeed = config.ContactSpeed;
            definition.transformWriteMode = PhysicsWorld.TransformWriteMode.Off;
            definition.preSolveCallbacks = true;
            definition.drawOptions = config.Drawing;
            var world = PhysicsWorld.Create(definition);
            if (!world.isValid)
                throw new InvalidOperationException("The low-level 2D physics created no world: Project Settings → Physics 2D must not bypass it.");

            world.warmStartingAllowed = config.WarmStarting;

            lifetime.OnTermination(() => world.Destroy(0));
            return new PhysicsSpace(world, step);
        }

        public void MakeOneWay(Lifetime lifetime, PhysicsShape shape)
        {
            shape.preSolveCallbacks = true;
            shape.callbackTarget = _oneWay;
            _oneWay.Add(lifetime, shape);
        }

        public void Step()
        {
            World.Simulate(_step.Duration);
            _stepped.Fire(Unit.Instance);
        }
    }
}
