using System;
using System.Collections.Generic;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Audio.View.Playback
{
    public sealed class SoundPlayer : ISoundPlayer
    {
        private readonly SoundPlayerConfig _config;
        private readonly Voice[] _voices;
        private readonly Dictionary<Sound, int> _lastClips = new();

        private int _next;

        public SoundPlayer(Lifetime lifetime, SoundPlayerConfig config)
        {
            _config = config;

            var root = new GameObject("Sounds");
            lifetime.OnTermination(() => Object.Destroy(root));

            _voices = new Voice[config.Voices];
            for (var index = 0; index < _voices.Length; index++)
                _voices[index] = new Voice(lifetime, root.transform);
        }

        public void Play(Sound sound, Vector2 position) =>
            PlayDelayed(sound, position, 0f);

        public void PlayDelayed(Sound sound, Vector2 position, float delay) =>
            NextVoice().Play(NextClip(sound), sound, position, delay, _config);

        public void Play(Lifetime lifetime, Sound sound, Transform anchor)
        {
            var voice = NextVoice();
            var playback = voice.Play(NextClip(sound), sound, anchor.position, 0f, _config);
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, playback.Intersect(lifetime))
                .Subscribe(_ => voice.Follow(anchor));
        }

        private Voice NextVoice()
        {
            for (var offset = 0; offset < _voices.Length; offset++)
            {
                var index = (_next + offset) % _voices.Length;
                if (!_voices[index].Busy)
                    return Take(index);
            }

            return Take(_next); //TODO: когда заняты все голоса, самый старый звук обрывается без затухания
        }

        private Voice Take(int index)
        {
            _next = (index + 1) % _voices.Length;
            return _voices[index];
        }

        private AudioClip NextClip(Sound sound)
        {
            var count = sound.Clips.Length;
            if (count == 0)
                throw new InvalidOperationException("Sound has no clips.");

            var index = count > 1 && _lastClips.TryGetValue(sound, out var last)
                ? (last + Random.Range(1, count)) % count
                : Random.Range(0, count);
            _lastClips[sound] = index;

            var clip = sound.Clips[index];
            if (clip == null)
                throw new InvalidOperationException($"Sound clip {index} is missing.");

            return clip;
        }

        private sealed class Voice
        {
            public bool Busy => _source.isPlaying;

            private readonly AudioSource _source;
            private readonly SequentialLifetimes _playbacks;

            public Voice(Lifetime lifetime, Transform root)
            {
                var voice = new GameObject("Voice");
                voice.transform.SetParent(root, false);
                _source = voice.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.dopplerLevel = 0f;
                _source.rolloffMode = AudioRolloffMode.Linear;
                _playbacks = new SequentialLifetimes(lifetime);
            }

            public Lifetime Play(AudioClip clip, Sound sound, Vector2 position, float delay, SoundPlayerConfig config)
            {
                var playback = _playbacks.Next();

                _source.Stop();
                _source.transform.position = position;
                _source.clip = clip;
                _source.volume = sound.Volume;
                _source.pitch = 1f + Random.Range(-sound.PitchVariation, sound.PitchVariation);
                _source.spatialBlend = config.SpatialBlend;
                _source.spread = config.Spread;
                _source.minDistance = config.MinDistance;
                _source.maxDistance = config.MaxDistance;
                _source.PlayDelayed(delay);

                return playback;
            }

            public void Follow(Transform anchor)
            {
                if (_source.isPlaying)
                    _source.transform.position = anchor.position;
                else
                    _playbacks.TerminateCurrent();
            }
        }
    }
}
