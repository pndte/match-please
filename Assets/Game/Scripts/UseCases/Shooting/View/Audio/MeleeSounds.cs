using System;
using Bw.UseCases.Audio.View.Playback;

namespace Bw.UseCases.Shooting.View.Audio
{
    [Serializable]
    public class MeleeSounds
    {
        public Sound Swing = new();
        public Sound BodyHit = new();
        public Sound SurfaceHit = new();
    }
}
