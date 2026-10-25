using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Prediction;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Network.Prediction
{
    public sealed class PushEffectRulesTests
    {
        private static readonly MovementState Standing = new(new BodyState(new Vector2(1f, 0f), new Vector2(2f, -1f), 0.5f, true), new JumpState(false, 0, 0));

        [Test]
        public void ImpulseOfTheEffectPushesTheBody()
        {
            var rules = new PushEffectRules<MovementState, float>(new ImpulsePerUnit(new Vector2(4f, 3f)), new FixedBodyConfig { PushLift = 1f });

            var pushed = rules.Apply(Standing, 2f);

            Assert.That(pushed.Body.HorizontalPush, Is.EqualTo(0.5f + 8f));
            Assert.That(pushed.Body.Velocity, Is.EqualTo(new Vector2(2f, -1f + 6f)));
        }

        [Test]
        public void BodyLiftScalesOnlyTheUpwardPart()
        {
            var rules = new PushEffectRules<MovementState, float>(new ImpulsePerUnit(new Vector2(4f, 3f)), new FixedBodyConfig { PushLift = 0.5f });

            var pushed = rules.Apply(Standing, 1f);

            Assert.That(pushed.Body.HorizontalPush, Is.EqualTo(0.5f + 4f));
            Assert.That(pushed.Body.Velocity.y, Is.EqualTo(-1f + 1.5f));
        }

        [Test]
        public void PushKeepsThePositionTheGroundAndTheJump()
        {
            var rules = new PushEffectRules<MovementState, float>(new ImpulsePerUnit(Vector2.one), new FixedBodyConfig());

            var pushed = rules.Apply(Standing, 1f);

            Assert.That(pushed.Body.Position, Is.EqualTo(Standing.Body.Position));
            Assert.That(pushed.Body.Grounded, Is.EqualTo(Standing.Body.Grounded));
            Assert.That(pushed.Jump, Is.EqualTo(Standing.Jump));
        }

        [Test]
        public void ZeroEffectChangesNothing()
        {
            var rules = new PushEffectRules<MovementState, float>(new ImpulsePerUnit(new Vector2(4f, 3f)), new FixedBodyConfig());

            Assert.That(rules.Apply(Standing, 0f), Is.EqualTo(Standing));
        }

        private sealed class ImpulsePerUnit : IPushRules<float>
        {
            private readonly Vector2 _unit;

            public ImpulsePerUnit(Vector2 unit)
            {
                _unit = unit;
            }

            public Vector2 Impulse(float effect) =>
                _unit * effect;
        }
    }
}
