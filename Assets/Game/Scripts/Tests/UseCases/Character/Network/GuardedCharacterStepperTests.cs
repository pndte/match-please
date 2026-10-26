using System.Collections.Generic;
using Bw.Entities.Simulation;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class GuardedCharacterStepperTests
    {
        [Test]
        public void GuardStepsWithTheStanceBeforeTheCharacter()
        {
            var order = new List<string>();
            var guard = new StanceSteps(order);
            var steps = new GuardedCharacterStepper(new ControlsSteps(order), guard);

            steps.Step(new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, 135f)));

            Assert.That(order, Is.EqualTo(new[] { "guard True 135", "character" }));
        }

        [Test]
        public void CharacterStepsWithTheWholeControls()
        {
            var order = new List<string>();
            var character = new ControlsSteps(order);
            var steps = new GuardedCharacterStepper(character, new StanceSteps(order));
            var controls = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, 135f));

            steps.Step(controls);

            Assert.That(character.Stepped, Has.Count.EqualTo(1));
            Assert.That(character.Stepped[0].Hands, Is.EqualTo(Controls.Firing));
            Assert.That(character.Stepped[0].Guard.Raised, Is.True);
        }

        private sealed class StanceSteps : ISteppable<GuardStance>
        {
            private readonly List<string> _order;

            public StanceSteps(List<string> order)
            {
                _order = order;
            }

            public void Step(GuardStance input) =>
                _order.Add($"guard {input.Raised} {input.Aim}");
        }

        private sealed class ControlsSteps : ISteppable<CharacterControls>
        {
            public List<CharacterControls> Stepped { get; } = new();

            private readonly List<string> _order;

            public ControlsSteps(List<string> order)
            {
                _order = order;
            }

            public void Step(CharacterControls input)
            {
                Stepped.Add(input);
                _order.Add("character");
            }
        }
    }
}
