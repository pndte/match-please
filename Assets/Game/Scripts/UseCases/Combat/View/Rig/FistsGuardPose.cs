using Bw.UseCases.Combat.View.Abstractions;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Rig
{
    public sealed class FistsGuardPose
    {
        private const float ClipLength = 0.3f;
        private const float HoldTime = 0.125f;

        private static readonly int BlockingId = Animator.StringToHash(MeleeRigParameters.Blocking);
        private static readonly int BlockTimeId = Animator.StringToHash(MeleeRigParameters.BlockTime);

        private readonly Animator _rig;

        private float _time;

        public FistsGuardPose(Lifetime lifetime, Animator rig, IWeaponCarryViewModel carry)
        {
            _rig = rig;

            carry.Drawn.WhenTrue(lifetime, drawnLifetime =>
                carry.Holder.Guard.Up.WhenTrue(drawnLifetime, Raise));
        }

        private void Raise(Lifetime upLifetime)
        {
            DOTween.Kill(this);
            if (_time >= 1f)
                Pose(0f);

            _rig.SetBool(BlockingId, true);
            Play(HoldTime / ClipLength);
            upLifetime.OnTermination(Lower);
        }

        private void Lower()
        {
            DOTween.Kill(this);
            Play(1f).OnComplete(() => _rig.SetBool(BlockingId, false));
        }

        private Tween Play(float to) =>
            DOTween.To(() => _time, Pose, to, Mathf.Abs(to - _time) * ClipLength)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .SetLink(_rig.gameObject);

        private void Pose(float time)
        {
            _time = time;
            _rig.SetFloat(BlockTimeId, time);
        }
    }
}
