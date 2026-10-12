using System;
using Bw.UseCases.Audio.View.Playback;

namespace Bw.UseCases.Crates.View.Audio
{
    [Serializable]
    public sealed class CrateSoundsConfig
    {
        public Sound Hit = new();
        public Sound Break = new();
    }
}
