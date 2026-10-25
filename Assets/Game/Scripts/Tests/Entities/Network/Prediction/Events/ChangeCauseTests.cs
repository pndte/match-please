using Bw.Entities.Network.Prediction.Events;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class ChangeCauseTests
    {
        [Test]
        public void PredictedCauseCarriesItsEffect()
        {
            Assert.That(Described(ChangeCause<int, float>.Predicted(2.5f)), Is.EqualTo("predicted 2.5"));
        }

        [Test]
        public void AuthoritativeCauseCarriesTheServerState()
        {
            Assert.That(Described(ChangeCause<int, float>.Authoritative(70)), Is.EqualTo("authoritative 70"));
        }

        [Test]
        public void CorrectionCarriesNothing()
        {
            Assert.That(Described(ChangeCause<int, float>.Correction), Is.EqualTo("correction"));
        }

        [Test]
        public void DefaultCauseIsACorrection()
        {
            Assert.That(Described(default), Is.EqualTo("correction"));
        }

        [Test]
        public void ZeroEffectAndZeroStateKeepTheirKind()
        {
            Assert.That(Described(ChangeCause<int, float>.Predicted(0f)), Is.EqualTo("predicted 0"));
            Assert.That(Described(ChangeCause<int, float>.Authoritative(0)), Is.EqualTo("authoritative 0"));
        }

        [Test]
        public void SwitchPassesItsContextToTheBranch()
        {
            var cause = ChangeCause<int, float>.Authoritative(5);

            var result = cause.Switch(100, static (context, effect) => context + effect, static (context, state) => context + state, static context => -context);

            Assert.That(result, Is.EqualTo(105f));
        }

        private static string Described(ChangeCause<int, float> cause) =>
            cause.Switch(
                "",
                static (_, effect) => $"predicted {effect.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                static (_, state) => $"authoritative {state.ToString()}",
                static _ => "correction");
    }
}
