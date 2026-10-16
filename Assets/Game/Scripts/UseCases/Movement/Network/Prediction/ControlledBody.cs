using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class ControlledBody
    {
        public ControlledBody(Lifetime lifetime, IControlledBy controlledBy, PhysicsBody body)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                body.enabled = false;
                controlledLifetime.OnTermination(() =>
                {
                    if (body.isValid)
                        body.enabled = true;
                });
            });
        }
    }
}
