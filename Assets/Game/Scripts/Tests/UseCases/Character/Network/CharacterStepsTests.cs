using System;
using System.Collections.Generic;
using Bw.Entities.Simulation;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Requests;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class CharacterStepsTests
    {
        private GuardConfig _guard;
        private WeaponInventoryConfig _inventory;
        private CharacterTrigger _trigger;
        private List<string> _order;
        private Recorded<MovementInput> _movement;
        private Recorded<HandsInput> _hands;
        private CharacterSteps _steps;

        [SetUp]
        public void SetUp()
        {
            _guard = ScriptableObject.CreateInstance<GuardConfig>();
            _guard.SpeedShare = 0.3f;
            _guard.JumpHeightShare = 0.75f;
            _inventory = ScriptableObject.CreateInstance<WeaponInventoryConfig>();
            _trigger = new CharacterTrigger();
            _order = new List<string>();
            _movement = new Recorded<MovementInput>(() => _order.Add($"movement with the trigger {_trigger.Current.Pulled}"));
            _hands = new Recorded<HandsInput>(() => _order.Add("hands"));
            _steps = new CharacterSteps(_trigger, _movement, _hands, new MovementControlsForm(_guard), new HandsControlsForm(_inventory));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_guard);
            Object.DestroyImmediate(_inventory);
        }

        [Test]
        public void TriggerIsTakenBeforeMovementAndHandsStep()
        {
            _steps.Step(Controls.RunningAndFiring);

            Assert.That(_order, Is.EqualTo(new[] { "movement with the trigger True", "hands" }));
            Assert.That(_trigger.Current, Is.EqualTo(Controls.Firing.Trigger));
        }

        [Test]
        public void MovementStepsByTheKeys()
        {
            _steps.Step(new CharacterControls(new MovementControls(-0.5f, true, false, true), Controls.Firing, GuardStance.Lowered));

            Assert.That(_movement.Inputs[0].Horizontal, Is.EqualTo(-0.5f));
            Assert.That(_movement.Inputs[0].JumpPressed, Is.True);
            Assert.That(_movement.Inputs[0].JumpHeld, Is.False);
            Assert.That(_movement.Inputs[0].Down, Is.True);
            Assert.That(_movement.Inputs[0].Slowdown, Is.EqualTo(Slowdown.None));
        }

        [Test]
        public void RaisedGuardSlowsTheMovement()
        {
            _steps.Step(new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, 0f)));

            Assert.That(_movement.Inputs[0].Slowdown.Speed, Is.EqualTo(0.7f).Within(1e-6f));
            Assert.That(_movement.Inputs[0].Slowdown.JumpHeight, Is.EqualTo(0.25f).Within(1e-6f));
        }

        [Test]
        public void HandsStepByTheAimTriggerAndReload()
        {
            _steps.Step(new CharacterControls(Controls.Running, new HandsControls(75f, Controls.Firing.Trigger, true, DrawControl.Keep), GuardStance.Lowered));

            Assert.That(_hands.Inputs[0].Weapon.Aim, Is.EqualTo(75f));
            Assert.That(_hands.Inputs[0].Weapon.Pulled, Is.True);
            Assert.That(_hands.Inputs[0].Weapon.ReloadPressed, Is.True);
            Assert.That(_hands.Inputs[0].Weapon.Guarded, Is.False);
            Assert.That(_hands.Inputs[0].Choice, Is.EqualTo(DrawChoice.Keep));
        }

        [Test]
        public void RaisedGuardGuardsTheHands()
        {
            _steps.Step(new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, 0f)));

            Assert.That(_hands.Inputs[0].Weapon.Guarded, Is.True);
        }

        [Test]
        public void SlotChoiceDrawsTheSlotOfTheLayout()
        {
            _steps.Step(new CharacterControls(Controls.Running, new HandsControls(0f, default, false, DrawControl.ToSlot(1)), GuardStance.Lowered));

            Assert.That(_hands.Inputs[0].Choice, Is.EqualTo(DrawChoice.ToSlot(_inventory.Slots[1])));
        }

        [Test]
        public void EveryControlsStepsBothOnce()
        {
            _steps.Step(Controls.RunningAndFiring);
            _steps.Step(Controls.RunningAndFiring);

            Assert.That(_movement.Inputs, Has.Count.EqualTo(2));
            Assert.That(_hands.Inputs, Has.Count.EqualTo(2));
        }

        private sealed class Recorded<TInput> : ISteppable<TInput> where TInput : struct
        {
            public List<TInput> Inputs { get; } = new();

            private readonly Action _onStep;

            public Recorded(Action onStep)
            {
                _onStep = onStep;
            }

            public void Step(TInput input)
            {
                Inputs.Add(input);
                _onStep();
            }
        }
    }
}
