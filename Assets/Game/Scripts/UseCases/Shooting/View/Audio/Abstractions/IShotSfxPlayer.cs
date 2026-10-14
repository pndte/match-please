using Bw.UseCases.Shooting.View.Abstractions;

namespace Bw.UseCases.Shooting.View.Audio.Abstractions
{
    public interface IShotSfxPlayer
    {
        public void Play();
        public void PlayImpact(PelletTrace trace, float arrival);
    }
}
