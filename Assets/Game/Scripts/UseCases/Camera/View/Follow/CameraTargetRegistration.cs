using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Camera.View.Follow
{
    public sealed class CameraTargetRegistration
    {
        public CameraTargetRegistration(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            Transform target,
            ICameraTargets targets)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                targets.AddLifetimed(controlledLifetime, target));
        }
    }
}
