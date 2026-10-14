using System;
using Bw.UseCases.Audio.View.Playback;

namespace Bw.UseCases.Shooting.View.Audio
{
    [Serializable]
    public class GunSounds
    {
        public Sound Shot = new();
        public ReloadCue[] Reload = Array.Empty<ReloadCue>();
    }
}
