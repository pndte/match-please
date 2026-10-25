using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Mortality.Network.Codecs
{
    public sealed class CausedVitalsCodecTests
    {
        private const float HalfStep = 0.5f / 32f;

        [TestCase(100f, 0f, LifeState.Alive)]
        [TestCase(72.5f, -27.5f, LifeState.Alive)]
        [TestCase(0f, -12.25f, LifeState.Dead)]
        [TestCase(60f, 10f, LifeState.Alive)]
        public void VitalsOnThirtySecondStepsSurviveExactly(float health, float change, LifeState state)
        {
            var decoded = RoundTrip(new CausedState<Vitals>(new Vitals(new HealthState(health, change), state), StateCause.None), out _);

            Assert.That(decoded.State.Health, Is.EqualTo(new HealthState(health, change)));
            Assert.That(decoded.State.State, Is.EqualTo(state));
        }

        [Test]
        public void HealthIsQuantizedToAThirtySecond()
        {
            var decoded = RoundTrip(new CausedState<Vitals>(new Vitals(new HealthState(33.3f, -12.142858f), LifeState.Alive), StateCause.None), out _);

            Assert.That(decoded.State.Health.Current, Is.EqualTo(33.3f).Within(HalfStep));
            Assert.That(decoded.State.Health.Change, Is.EqualTo(-12.142858f).Within(HalfStep));
        }

        [Test]
        public void StateWithoutCauseComesBackWithoutCause()
        {
            var decoded = RoundTrip(new CausedState<Vitals>(new Vitals(new HealthState(100f, 0f), LifeState.Alive), StateCause.None), out var size);

            Assert.That(CauseOf(decoded), Is.Null);
            Assert.That(size, Is.EqualTo(5));
        }

        [Test]
        public void ActionCauseComesBackWithItsInitiatorAndTick()
        {
            var cause = StateCause.Of(new ActionId(7UL, 1234));

            var decoded = RoundTrip(new CausedState<Vitals>(new Vitals(new HealthState(70f, -30f), LifeState.Alive), cause), out _);

            Assert.That(CauseOf(decoded), Is.EqualTo(new ActionId(7UL, 1234)));
        }

        private static CausedState<Vitals> RoundTrip(CausedState<Vitals> state, out int size)
        {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new CausedVitalsCodec { Value = state });
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out CausedVitalsCodec decoded);
            size = writer.Length;
            return decoded.Value;
        }

        private static ActionId? CauseOf(CausedState<Vitals> state)
        {
            ActionId? found = null;
            state.Cause.Switch(0, static _ => { }, (_, action) => found = action);
            return found;
        }
    }
}
