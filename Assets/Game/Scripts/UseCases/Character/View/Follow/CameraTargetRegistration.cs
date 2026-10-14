using Bw.UseCases.Camera.View.Follow;
using Bw.UseCases.Character.View.Player;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Follow
{
    public sealed class CameraTargetRegistration
    {
        public CameraTargetRegistration(
            Lifetime lifetime,
            IPlayerViewModel player,
            Transform target,
            ICameraTargets targets)
        {
            player.Controlled.WhenTrue(lifetime, controlledLifetime =>
                targets.AddLifetimed(controlledLifetime, target));
        }
    }
}
