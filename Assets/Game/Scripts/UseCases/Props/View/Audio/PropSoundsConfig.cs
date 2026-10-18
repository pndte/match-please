using System;
using Bw.UseCases.Audio.View.Playback;

namespace Bw.UseCases.Props.View.Audio
{
    [Serializable]
    public sealed class PropSoundsConfig
    {
        public Sound Throw = new();
        public Sound Flight = new();
        public Sound Land = new();
    }
}
