using Bw.Entities.Network.Rollback;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedBodyType
    {
        private readonly PhysicsBody _body;

        public PredictedBodyType(Lifetime lifetime, IPredictedBy predictedBy, PhysicsBody body)
        {
            _body = body;

            predictedBy.Me.Advise(lifetime, Switch);
        }

        private void Switch(bool predicted)
        {
            _body.type = predicted ? PhysicsBody.BodyType.Dynamic : PhysicsBody.BodyType.Kinematic;
            if (predicted)
                return;

            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }
    }
}
