using System;
using Bw.UseCases.Shooting.View.Abstractions;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Recoil
{
    public sealed class WeaponRecoilView
    {
        private readonly Transform _visual;
        private readonly WeaponRecoilConfig _config;

        public WeaponRecoilView(Lifetime lifetime, ISeenShots shots, WeaponRecoilConfig config, Transform visual)
        {
            _visual = visual;
            _config = config;

            shots.Seen.Advise(lifetime, _ => Kick());
        }

        private void Kick()
        {
            Punch(_config.Kick, new Vector3(-_config.Kick.Amount, 0f, 0f), (offset, seconds) => _visual.DOBlendableLocalMoveBy(offset, seconds));
            Punch(_config.Tilt, new Vector3(0f, 0f, _config.Tilt.Amount * Mathf.Sign(_visual.localScale.y)), (offset, seconds) => _visual.DOBlendableLocalRotateBy(offset, seconds));
        }

        private void Punch(RecoilPunch punch, Vector3 offset, Func<Vector3, float, Tweener> by) =>
            DOTween.Sequence()
                .Append(by(offset, punch.OutTime).SetEase(RecoilPunchExtensions.OutEase))
                .Append(by(-offset, punch.ReturnTime).SetEase(RecoilPunchExtensions.ReturnEase, punch.Overshoot))
                .SetLink(_visual.gameObject);
    }
}
