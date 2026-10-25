using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.UseCases.Movement.Network.Prediction;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Movement.Network.Prediction
{
    public sealed class CorrectionDecayTests
    {
        private const float DecaySeconds = 0.2f;

        private ManualClock _clock;
        private ViewProgress _progress;
        private CorrectionDecay _decay;

        [SetUp]
        public void SetUp()
        {
            _clock = new ManualClock(1f / 30f);
            _progress = new ViewProgress();
            _decay = new CorrectionDecay(_clock, _progress, DecaySeconds);
            _clock.Tick(100);
        }

        [Test]
        public void DecayNeverStartedHasNothingLeft()
        {
            Assert.That(_decay.Remaining, Is.Zero);
            Assert.That(_decay.Rate, Is.Zero);
        }

        [Test]
        public void RestartedDecayHasTheWholeCorrectionLeft()
        {
            _decay.Restart();

            Assert.That(_decay.Remaining, Is.EqualTo(1f));
            Assert.That(_decay.Rate, Is.Zero);
        }

        [Test]
        public void HalfTheDecayTimeLeavesHalfTheCorrection()
        {
            _decay.Restart();

            _clock.Advance(3);

            Assert.That(_decay.Remaining, Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(_decay.Rate, Is.EqualTo(-6f * 0.5f * 0.5f / DecaySeconds).Within(1e-3f));
        }

        [Test]
        public void WholeDecayTimeLeavesNothing()
        {
            _decay.Restart();

            _clock.Advance(6);

            Assert.That(_decay.Remaining, Is.Zero.Within(1e-6f));
            Assert.That(_decay.Rate, Is.Zero);
        }

        [Test]
        public void ProgressInsideTheTickCountsTowardTheDecay()
        {
            _decay.Restart();

            _progress.Progress = 0.5f;

            Assert.That(_decay.Remaining, Is.LessThan(1f));
            Assert.That(_decay.Remaining, Is.GreaterThan(0.9f));
        }

        [Test]
        public void RemainingOnlyShrinks()
        {
            _decay.Restart();
            var previous = _decay.Remaining;
            for (var step = 1; step <= 40; step++)
            {
                _progress.Progress = step % 4 / 4f;
                if (step % 4 == 0)
                    _clock.Advance();

                Assert.That(_decay.Remaining, Is.LessThanOrEqualTo(previous));
                previous = _decay.Remaining;
            }
        }

        [Test]
        public void RateIsHowFastTheRemainingShrinksPerSecond()
        {
            _decay.Restart();
            _clock.Advance(2);
            var before = _decay.Remaining;
            var rate = _decay.Rate;

            _progress.Progress = 0.01f;

            var perSecond = (_decay.Remaining - before) / (0.01f * _clock.Duration);
            Assert.That(perSecond, Is.EqualTo(rate).Within(0.05f));
        }

        [Test]
        public void SecondRestartBeginsTheDecayAgain()
        {
            _decay.Restart();
            _clock.Advance(4);

            _decay.Restart();

            Assert.That(_decay.Remaining, Is.EqualTo(1f));
        }

        private sealed class ViewProgress : IInterpolationTicks
        {
            public float Progress { get; set; }
            public double InterpolationTick => 0d;
        }
    }
}
