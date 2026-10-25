using Bw.Entities.Network.Rollback;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedBodyPose<TState> where TState : struct, IPlacedState<TState>
    {
        private readonly PhysicsBody _body;

        public PredictedBodyPose(
            Lifetime lifetime,
            IPredictedBy predictedBy,
            IReadonlySimulation<TState> simulation,
            PhysicsBody body)
        {
            _body = body;

            predictedBy.Me.WhenTrue(lifetime, predictedLifetime => simulation.State.Advise(predictedLifetime, Place));
        }

        private void Place(TState state) =>
            _body.transform = new PhysicsTransform(state.Position, new PhysicsRotate(state.Angle * Mathf.Deg2Rad));
    }
}
