using Bw.UseCases.Movement.View.Animation.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Animation
{
    public sealed class AnimationFootsteps : MonoBehaviour, IFootsteps
    {
        private const float DominantWeight = 0.5f;

        private readonly Signal<Unit> _stepped = new();

        public ISource<Unit> Stepped => _stepped;

        private void Step(AnimationEvent step)
        {
            if (step.animatorClipInfo.weight >= DominantWeight)
                _stepped.Fire(Unit.Instance);
        }
    }
}
