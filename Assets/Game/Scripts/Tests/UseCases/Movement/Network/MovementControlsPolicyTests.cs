using Bw.UseCases.Movement.Network;
using Bw.UseCases.Movement.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Movement.Network
{
    public sealed class MovementControlsPolicyTests
    {
        private readonly MovementControlsPolicy _policy = new();

        [TestCase(0f)]
        [TestCase(1f)]
        [TestCase(-1f)]
        [TestCase(5f)]
        public void FiniteRunIsValid(float horizontal)
        {
            Assert.That(_policy.IsValid(new MovementControls(horizontal, false, false, false)), Is.True);
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InfiniteRunIsInvalid(float horizontal)
        {
            Assert.That(_policy.IsValid(new MovementControls(horizontal, true, true, true)), Is.False);
        }
    }
}
