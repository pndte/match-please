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
        private readonly ISweptBodyCollection _sweeps;
        private readonly OneWayContacts _oneWay = new();
        private readonly Signal<Unit> _stepped = new();

        private PhysicsSpace(PhysicsWorld world, ISimulationStep step, ISweptBodyCollection sweeps)
        {
            World = world;
            _step = step;
            _sweeps = sweeps;
        }

        public static PhysicsSpace Create(
            Lifetime lifetime,
            PhysicsConfig config,
            ISimulationStep step,
            ISweptBodyCollection sweeps)
        {
            var definition = PhysicsWorldDefinition.defaultDefinition;
            definition.simulateType = PhysicsWorld.SimulationType.Script;
            definition.gravity = Physics2D.gravity;
            definition.simulationSubSteps = config.SubSteps;
            definition.simulationWorkers = config.Workers;
            definition.contactFrequency = config.ContactFrequency;
            definition.contactDamping = config.ContactDamping;
            definition.contactSpeed = config.ContactSpeed;
            definition.transformWriteMode = PhysicsWorld.TransformWriteMode.Off;
            definition.preSolveCallbacks = true;
            definition.drawOptions = config.Drawing;
            var world = PhysicsWorld.Create(definition);
            if (!world.isValid)
                throw new InvalidOperationException("The low-level 2D physics created no world: Project Settings → Physics 2D must not bypass it.");

            lifetime.OnTermination(() => world.Destroy(0));
            return new PhysicsSpace(world, step, sweeps);
        }

        public void MakeOneWay(Lifetime lifetime, PhysicsShape shape)
        {
            shape.preSolveCallbacks = true;
            shape.callbackTarget = _oneWay;
            _oneWay.Add(lifetime, shape);
        }

        public void Step()
        {
            var duration = _step.Duration;
            for (var index = 0; index < _sweeps.Count; index++)
                _sweeps[index].Sweep(duration);

            World.Simulate(duration);
            for (var index = 0; index < _sweeps.Count; index++)
                _sweeps[index].Settle();

            _stepped.Fire(Unit.Instance);
        }
    }
}
