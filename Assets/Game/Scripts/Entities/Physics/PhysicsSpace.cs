using System;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    public sealed class PhysicsSpace : IPhysicsSpace, ISteppable
    {
        public PhysicsWorld World { get; }

        private readonly ISimulationStep _step;

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
            definition.transformWriteMode = PhysicsWorld.TransformWriteMode.Off;
            definition.drawOptions = config.Drawing;
            var world = PhysicsWorld.Create(definition);
            if (!world.isValid)
                throw new InvalidOperationException("The low-level 2D physics created no world: Project Settings → Physics 2D must not bypass it.");

            lifetime.OnTermination(() => world.Destroy(0));
            return new PhysicsSpace(world, step);
        }

        public void Step() =>
            World.Simulate(_step.Duration);
    }
}
