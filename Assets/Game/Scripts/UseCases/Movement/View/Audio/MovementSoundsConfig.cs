using System;
using Bw.UseCases.Audio.View.Playback;

namespace Bw.UseCases.Movement.View.Audio
{
    [Serializable]
    public sealed class MovementSoundsConfig
    {
        public Sound Step = new();
        public Sound Jump = new();
        public Sound Land = new();
    }
}
