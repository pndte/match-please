using System.Collections.Generic;
using Bw.Entities.Simulation;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;
using JetBrains.Collections.Viewable;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class RootedCharacterStepsTests
    {
        private static readonly CharacterControls Played = new(new MovementControls(1f, true, true, true), Controls.Firing, new GuardStance(true, 20f));

        private ControlsSteps _character;
        private HandsHolding _hands;
        private RootedCharacterSteps _steps;

        [SetUp]
        public void SetUp()
        {
            _character = new ControlsSteps();
            _hands = new HandsHolding();
            _steps = new RootedCharacterSteps(_character, _hands);
        }

        [Test]
        public void EmptyHandsPassTheControlsAsTheyAre()
        {
            _steps.Step(Played);

            Assert.That(_character.Stepped, Is.EqualTo(new[] { Played }));
        }

        [Test]
        public void WeaponThatDoesNotRootPassesTheControlsAsTheyAre()
        {
            _hands.Held = new Weapon { Rooting = false };

            _steps.Step(Played);

            Assert.That(_character.Stepped, Is.EqualTo(new[] { Played }));
        }

        [Test]
        public void RootingWeaponStopsTheMovementControls()
        {
            _hands.Held = new Weapon { Rooting = true };

            _steps.Step(Played);

            Assert.That(_character.Stepped[0].Movement, Is.EqualTo(default(MovementControls)));
        }

        [Test]
        public void RootingWeaponKeepsTheHandsAndTheGuard()
        {
            _hands.Held = new Weapon { Rooting = true };

            _steps.Step(Played);

            Assert.That(_character.Stepped[0].Hands, Is.EqualTo(Played.Hands));
            Assert.That(_character.Stepped[0].Guard, Is.EqualTo(Played.Guard));
        }

        [Test]
        public void RootIsAskedOnEveryStep()
        {
            var weapon = new Weapon { Rooting = true };
            _hands.Held = weapon;
            _steps.Step(Played);
            weapon.Rooting = false;

            _steps.Step(Played);

            Assert.That(_character.Stepped[0].Movement.Horizontal, Is.Zero);
            Assert.That(_character.Stepped[1].Movement.Horizontal, Is.EqualTo(1f));
        }

        private sealed class ControlsSteps : ISteppable<CharacterControls>
        {
            public List<CharacterControls> Stepped { get; } = new();

            public void Step(CharacterControls input) =>
                Stepped.Add(input);
        }

        private sealed class HandsHolding : IReadonlyHands
        {
            public IWeapon Held { get; set; }
            public IReadonlyProperty<HandsState> State { get; } = new ViewableProperty<HandsState>(default);
            public HandsState Previous => default;
            public ISource<HandsState> Stepped { get; } = new Signal<HandsState>();
            public bool Busy => Held != null;
            public HoldPose Pose => default;
            public GripPose Grip => default;

            public bool TryFindInHands(out IWeapon weapon)
            {
                weapon = Held;
                return Held != null;
            }
        }

        private sealed class Weapon : IWeapon
        {
            public ISource<WeaponShot> Fired { get; } = new Signal<WeaponShot>();
            public bool Stowable => !Rooting;
            public bool Rooting { get; set; }

            public void Step(WeaponInput input)
            {
            }
        }
    }
}
