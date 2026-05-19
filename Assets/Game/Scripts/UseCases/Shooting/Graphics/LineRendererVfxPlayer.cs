using Bw.UseCases.Shooting.Graphics;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Bw.UseCases.Shooting
{
    public class LineRendererVfxPlayer : IShotVfxPlayer
    {
        private readonly LineRendererVfxConfig _raycastShootConfig;
        private readonly LineRenderer _trailPrefab;

        public LineRendererVfxPlayer(
            LineRendererVfxConfig raycastShootConfig, 
            LineRenderer trailPrefab)
        {
            _raycastShootConfig = raycastShootConfig;
            _trailPrefab = trailPrefab;
        }
        
        public async UniTaskVoid Play(Vector3 from, Vector3 to)
        {
            var trail = Object.Instantiate(_trailPrefab, from, Quaternion.identity);
            trail.enabled = true;

            var distance = Vector3.Distance(from, to);
            var duration = distance / _raycastShootConfig.TrailSpeed;

            float t = 0;
            await DOTween.To(() => t, x => t = x, 1f, duration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    trail.SetPosition(1, Vector3.Lerp(from, to, t));
                    trail.SetPosition(0, Vector3.Lerp(from, to, Mathf.Max(0, t - _raycastShootConfig.TrailLength)));
                })
                .ToUniTask();

            Object.Destroy(trail.gameObject);
        }
    }
}