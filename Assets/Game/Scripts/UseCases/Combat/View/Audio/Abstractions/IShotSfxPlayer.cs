using Bw.UseCases.Combat.View.Abstractions;

namespace Bw.UseCases.Combat.View.Audio.Abstractions
{
    public interface IShotSfxPlayer
    {
        public void Play();
        public void PlayImpact(PelletTrace trace, float arrival);
    }
}
