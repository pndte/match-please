using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Tests.Entities.Network.Doubles;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network.Codecs;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class WireRoundedSamplerTests
    {
        private static readonly CharacterControls Raw = new(
            Controls.Running,
            new HandsControls(30.1234f, new TriggerControls(true, 2.3456f, 1.789f, Controls.Press), false, DrawControl.Keep),
            new GuardStance(true, -77.777f));

        private WireRoundedSampler<CharacterControls, TickedCharacterControlsCodec> _sampler;

        [SetUp]
        public void SetUp() =>
            _sampler = new WireRoundedSampler<CharacterControls, TickedCharacterControlsCodec>(new FixedSampler());

        [Test]
        public void SampleIsRoundedLikeTheWireRoundsIt()
        {
            var sampled = _sampler.Sample();

            Assert.That(sampled.Hands.Aim, Is.Not.EqualTo(Raw.Hands.Aim));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(sampled.Hands.Aim, Raw.Hands.Aim)), Is.LessThan(0.01f));
            Assert.That(sampled.Guard.Aim, Is.Not.EqualTo(Raw.Guard.Aim));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(sampled.Guard.Aim, Raw.Guard.Aim)), Is.LessThan(0.01f));
        }

        [Test]
        public void SampledInputReachesTheServerUnchanged()
        {
            var sampled = _sampler.Sample();

            var received = Wire.RoundTrip(new TickedCharacterControlsCodec { Value = new TickedInput<CharacterControls>(9, sampled, sampled) }).Value;

            Assert.That(received.Input.Hands.Aim, Is.EqualTo(sampled.Hands.Aim));
            Assert.That(received.Input.Hands.Trigger.ViewDelay, Is.EqualTo(sampled.Hands.Trigger.ViewDelay));
            Assert.That(received.Input.Hands.Trigger.PredictedDelay, Is.EqualTo(sampled.Hands.Trigger.PredictedDelay));
            Assert.That(received.Input.Hands.Trigger.Press, Is.EqualTo(sampled.Hands.Trigger.Press));
            Assert.That(received.Input.Guard.Aim, Is.EqualTo(sampled.Guard.Aim));
            Assert.That(received.PreviousInput.Hands.Aim, Is.EqualTo(sampled.Hands.Aim));
            Assert.That(received.PreviousInput.Guard.Aim, Is.EqualTo(sampled.Guard.Aim));
        }

        [Test]
        public void InputOnTheWiresStepsIsLeftAsItIs()
        {
            var exact = new CharacterControls(
                Controls.Running,
                new HandsControls(90f, new TriggerControls(true, 2.5f, 1.5f, Controls.Press), false, DrawControl.Keep),
                new GuardStance(true, -45f));
            var sampler = new WireRoundedSampler<CharacterControls, TickedCharacterControlsCodec>(new FixedSampler(exact));

            var sampled = sampler.Sample();

            Assert.That(sampled.Movement.Horizontal, Is.EqualTo(exact.Movement.Horizontal));
            Assert.That(sampled.Hands.Aim, Is.EqualTo(exact.Hands.Aim));
            Assert.That(sampled.Hands.Trigger.ViewDelay, Is.EqualTo(exact.Hands.Trigger.ViewDelay));
            Assert.That(sampled.Hands.Trigger.Press, Is.EqualTo(exact.Hands.Trigger.Press));
            Assert.That(sampled.Guard.Aim, Is.EqualTo(exact.Guard.Aim));
        }

        private sealed class FixedSampler : IInputSampler<CharacterControls>
        {
            private readonly CharacterControls _controls;

            public FixedSampler() : this(Raw)
            {
            }

            public FixedSampler(CharacterControls controls)
            {
                _controls = controls;
            }

            public CharacterControls Sample() =>
                _controls;
        }
    }
}
