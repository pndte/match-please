using System;
using Bw.UseCases.Audio.View.Playback;

namespace Bw.UseCases.Character.View.Audio
{
    [Serializable]
    public sealed class CharacterSoundsConfig
    {
        public Sound BodyHit = new();
    }
}
