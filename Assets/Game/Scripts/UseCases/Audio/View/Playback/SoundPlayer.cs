using System;
using System.Collections.Generic;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Audio.View.Playback
{
    public sealed class SoundPlayer : ISoundPlayer
    {
        private IPool<Voice> Voices => _pools.For<Voice>(_config.Voice);

        private readonly Lifetime _lifetime;
        private readonly IPrefabPools _pools;
        private readonly SoundPlayerConfig _config;
        private readonly Dictionary<Sound, int> _lastClips = new();

        public SoundPlayer(Lifetime lifetime, IPrefabPools pools, SoundPlayerConfig config)
        {
            _lifetime = lifetime;
            _pools = pools;
            _config = config;
        }

        public void Play(Sound sound, Vector2 position)
        {
            var clip = NextClip(sound);
            Voices.OneShot(_lifetime).Play(clip, sound, position);
        }

        public void Play(Lifetime lifetime, Sound sound, Transform anchor)
        {
            var clip = NextClip(sound);
            var voice = Voices.OneShot(_lifetime);
            voice.Play(clip, sound, anchor.position);
            voice.Follow(lifetime, anchor);
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
    }
}
