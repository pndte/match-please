using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Movement.Network.Prediction;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class SmoothedAimViewTests
    {
        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private FakeCorrections _corrections;
        private RecordingView<GunState> _inner;
        private SmoothedAimView<GunState> _view;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock(0.1f);
            _corrections = new FakeCorrections();
            _inner = new RecordingView<GunState>();
            var config = new CorrectionSmoothingConfig { DecaySeconds = 0.2f, SnapAngle = 45f };
            _view = new SmoothedAimView<GunState>(_lifetime.Lifetime, _inner, _corrections, _clock, new StillInterpolation(), config);
            _clock.Tick(10);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void AimWithoutCorrectionsIsShownAsItIs()
        {
            _view.Show(Aimed(5f), Aimed(15f), 0.3f);

            Assert.That(_inner.Shown, Is.EqualTo(new[] { (Aimed(5f), Aimed(15f), 0.3f) }));
        }

        [Test]
        public void CorrectedAimIsDrawnWhereItWasAtFirst()
        {
            _corrections.Correct(Aimed(10f), Aimed(20f));

            _view.Show(Aimed(20f), Aimed(20f), 0f);

            Assert.That(ShownAim(), Is.EqualTo(10f).Within(1e-4f));
        }

        [Test]
        public void BothEndsOfTheShownStepAreTurned()
        {
            _corrections.Correct(Aimed(10f), Aimed(20f));

            _view.Show(Aimed(0f), Aimed(30f), 0.5f);

            Assert.That(_inner.Shown[0].from.Aim, Is.EqualTo(-10f).Within(1e-4f));
            Assert.That(_inner.Shown[0].to.Aim, Is.EqualTo(20f).Within(1e-4f));
            Assert.That(_inner.Shown[0].progress, Is.EqualTo(0.5f));
        }

        [Test]
        public void CorrectionFadesIntoTheTrueAimOverTheDecay()
        {
            _corrections.Correct(Aimed(10f), Aimed(20f));

            _clock.Tick(11);
            _view.Show(Aimed(20f), Aimed(20f), 0f);
            _clock.Tick(12);
            _view.Show(Aimed(20f), Aimed(20f), 0f);
            _clock.Tick(20);
            _view.Show(Aimed(20f), Aimed(20f), 0f);

            Assert.That(_inner.Shown[0].to.Aim, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(_inner.Shown[1].to.Aim, Is.EqualTo(20f).Within(1e-4f));
            Assert.That(_inner.Shown[2].to.Aim, Is.EqualTo(20f).Within(1e-4f));
        }

        [Test]
        public void CorrectionAboveTheSnapAngleIsShownAtOnce()
        {
            _corrections.Correct(Aimed(0f), Aimed(60f));

            _view.Show(Aimed(60f), Aimed(60f), 0f);

            Assert.That(ShownAim(), Is.EqualTo(60f).Within(1e-4f));
        }

        [Test]
        public void CorrectionsAddUpWhileTheLastOneFades()
        {
            _corrections.Correct(Aimed(10f), Aimed(20f));
            _clock.Tick(11);

            _corrections.Correct(Aimed(20f), Aimed(30f));
            _view.Show(Aimed(30f), Aimed(30f), 0f);

            Assert.That(ShownAim(), Is.EqualTo(15f).Within(1e-4f));
        }

        [Test]
        public void CorrectionsAddingUpBeyondTheSnapAngleSnap()
        {
            _corrections.Correct(Aimed(0f), Aimed(30f));
            _corrections.Correct(Aimed(30f), Aimed(60f));

            _view.Show(Aimed(60f), Aimed(60f), 0f);

            Assert.That(ShownAim(), Is.EqualTo(60f).Within(1e-4f));
        }

        [Test]
        public void CorrectionAcrossTheBackOfTheCircleTurnsTheShortWay()
        {
            _corrections.Correct(Aimed(170f), Aimed(-170f));

            _view.Show(Aimed(-170f), Aimed(-170f), 0f);

            Assert.That(ShownAim(), Is.EqualTo(-190f).Within(1e-4f));
        }

        private float ShownAim() =>
            _inner.Shown[_inner.Shown.Count - 1].to.Aim;

        private static GunState Aimed(float aim) =>
            new(aim, 3, 0, 0, 0);

        private sealed class FakeCorrections : IStateCorrections<GunState>
        {
            public ISource<StateCorrection<GunState>> Corrected => _corrected;

            private readonly Signal<StateCorrection<GunState>> _corrected = new();

            public void Correct(GunState before, GunState after) =>
                _corrected.Fire(new StateCorrection<GunState>(before, after));
        }

        private sealed class StillInterpolation : IInterpolationTicks
        {
            public float Progress => 0f;
            public double InterpolationTick => 0d;
        }
    }
}
