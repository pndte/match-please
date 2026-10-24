using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Network.Platforms
{
    public sealed class RiderRewinder : IRewindable
    {
        private readonly IRewindable _rewinder;
        private readonly IMovementBody _body;
        private readonly IPlatformRider _rider;
        private readonly ISimulationStep _step;

        public RiderRewinder(
            IRewindable rewinder,
            IMovementBody body,
            IPlatformRider rider,
            ISimulationStep step)
        {
            _rewinder = rewinder;
            _body = body;
            _rider = rider;
            _step = step;
        }

        public void Rewind(Lifetime lifetime, ShooterView view)
        {
            var seen = view.TickOf(_body.Position);
            _rewinder.Rewind(lifetime, view);
            var duration = (double)_step.Duration;
            var position = _body.Position;
            _body.Place(position + _rider.RideShift(position, _body.Angle, seen * duration, view.Predicted * duration));
        }
    }
}
