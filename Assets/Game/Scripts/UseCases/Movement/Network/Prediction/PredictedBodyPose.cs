using Bw.Entities.Network.Rollback;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedBodyPose<TState> where TState : struct, IPlacedState<TState>
    {
        private readonly IBodyPose _body;

        public PredictedBodyPose(
            Lifetime lifetime,
            IPredictedBy predictedBy,
            IReadonlySimulation<TState> simulation,
            IBodyPose body)
        {
            _body = body;

            predictedBy.Me.WhenTrue(lifetime, predictedLifetime => simulation.State.Advise(predictedLifetime, Place));
        }

        private void Place(TState state) =>
            _body.Place(state.Position, state.Angle);
    }
}
