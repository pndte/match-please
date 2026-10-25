using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Requests;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class MovementControlsFormTests
    {
        private GuardConfig _guard;
        private MovementControlsForm _form;

        [SetUp]
        public void SetUp()
        {
            _guard = ScriptableObject.CreateInstance<GuardConfig>();
            _guard.SpeedShare = 0.4f;
            _guard.JumpHeightShare = 0.9f;
            _form = new MovementControlsForm(_guard);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_guard);

        [TestCase(1f, true, true, false)]
        [TestCase(-0.375f, false, true, true)]
        [TestCase(0f, false, false, false)]
        public void KeysPassAsTheyAre(
            float horizontal,
            bool jumpPressed,
            bool jumpHeld,
            bool down)
        {
            var input = _form.InputOf(new CharacterControls(new MovementControls(horizontal, jumpPressed, jumpHeld, down), Controls.Firing, new GuardStance(true, 0f)));

            Assert.That(input.Horizontal, Is.EqualTo(horizontal));
            Assert.That(input.JumpPressed, Is.EqualTo(jumpPressed));
            Assert.That(input.JumpHeld, Is.EqualTo(jumpHeld));
            Assert.That(input.Down, Is.EqualTo(down));
        }

        [Test]
        public void LoweredStanceDoesNotSlowDown()
        {
            var input = _form.InputOf(new CharacterControls(Controls.Running, Controls.Firing, GuardStance.Lowered));

            Assert.That(input.Slowdown, Is.EqualTo(Slowdown.None));
        }

        [Test]
        public void RaisedStanceKeepsTheGuardsShareOfSpeedAndJumpHeight()
        {
            var input = _form.InputOf(new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, 120f)));

            Assert.That(1f - input.Slowdown.Speed, Is.EqualTo(0.4f).Within(1e-6f));
            Assert.That(1f - input.Slowdown.JumpHeight, Is.EqualTo(0.9f).Within(1e-6f));
        }

        [Test]
        public void ShippedGuardHalvesSpeedAndJumpHeight()
        {
            var form = new MovementControlsForm(AssetDatabase.LoadAssetAtPath<GuardConfig>("Assets/Resources/GuardConfig.asset"));

            var input = form.InputOf(new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(true, 0f)));

            Assert.That(input.Slowdown.Speed, Is.EqualTo(0.5f));
            Assert.That(input.Slowdown.JumpHeight, Is.EqualTo(0.5f));
        }
    }
}
