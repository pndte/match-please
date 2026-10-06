using Bw.Entities.Pool;
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
        private readonly Signal<Unit> _finished = new();
        private Lifetime _lifetime;

        public Voice Facade(Lifetime lifetime)
        {
            _lifetime = lifetime;
            lifetime.OnTermination(_source.Stop); //TODO: когда заняты все голоса, самый старый звук обрывается без затухания
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
    }
}
