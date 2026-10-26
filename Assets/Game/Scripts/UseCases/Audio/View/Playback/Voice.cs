using Bw.Entities.Pool;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;
using Unit = JetBrains.Core.Unit;

namespace Bw.UseCases.Audio.View.Playback
{
    public sealed class Voice : MonoBehaviour, IResource<Voice>, IOneShot
    {
        public ISource<Unit> Finished => _finished;

        [SerializeField] private AudioSource _source;
        [SerializeField] private AudioSource _tail;
        [SerializeField, Min(0f)] private float _fadeSeconds = 0.06f;
        private readonly Signal<Unit> _finished = new();
        private Lifetime _lifetime;

        public Voice Facade(Lifetime lifetime)
        {
            _lifetime = lifetime;
            lifetime.OnTermination(Release);
            return this;
        }

        public void Play(AudioClip clip, Sound sound, Vector2 position)
        {
            transform.position = position;
            _source.clip = clip;
            _source.volume = sound.Volume;
            _source.pitch = 1f + Random.Range(-sound.PitchVariation, sound.PitchVariation);
            _source.Play();
            Observable.EveryUpdate(UnityFrameProvider.Update, _lifetime)
                .Where(_ => !_source.isPlaying)
                .Take(1)
                .Subscribe(_ => _finished.Fire(Unit.Instance));
        }

        public void Follow(Lifetime lifetime, Transform anchor) =>
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, _lifetime.Intersect(lifetime))
                .Subscribe(_ => transform.position = anchor.position);

        private void Release()
        {
            if (_source.isPlaying)
                FadeOut();

            _source.Stop();
        }

        private void FadeOut()
        {
            _tail.DOKill();
            _tail.clip = _source.clip;
            _tail.pitch = _source.pitch;
            _tail.volume = _source.volume;
            _tail.Play();
            _tail.timeSamples = _source.timeSamples;
            DOTween.To(() => _tail.volume, volume => _tail.volume = volume, 0f, _fadeSeconds)
                .SetTarget(_tail)
                .OnComplete(_tail.Stop)
                .SetLink(gameObject);
        }
    }
}
