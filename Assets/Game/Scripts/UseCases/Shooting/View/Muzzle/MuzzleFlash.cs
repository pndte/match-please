using Bw.UseCases.Shooting.View.Abstractions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Bw.UseCases.Shooting.View.Muzzle
{
    public sealed class MuzzleFlash : MonoBehaviour, IMuzzleFlash
    {
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private Light2D _light;
        [SerializeField, Min(0f)] private float _intensity = 1.5f;
        [SerializeField, Min(0.01f)] private float _fade = 0.08f;

        public void Play()
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _particles.Play(true);
            _light.DOKill();
            _light.intensity = _intensity;
            _light.enabled = true;
            DOTween.To(() => _light.intensity, intensity => _light.intensity = intensity, 0f, _fade)
                .SetEase(Ease.OutQuad)
                .SetTarget(_light)
                .SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable)
                .OnComplete(() => _light.enabled = false);
        }
    }
}
