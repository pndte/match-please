using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Audio.View.Playback.Abstractions
{
    public interface ISoundPlayer
    {
        public void Play(Sound sound, Vector2 position);
        public void PlayDelayed(Sound sound, Vector2 position, float delay);
        public void Play(Lifetime lifetime, Sound sound, Transform anchor);
    }
}
