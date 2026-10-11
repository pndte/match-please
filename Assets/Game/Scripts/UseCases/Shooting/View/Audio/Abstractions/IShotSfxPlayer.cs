using Bw.UseCases.Shooting.Weapon;

namespace Bw.UseCases.Shooting.View.Audio.Abstractions
{
    public interface IShotSfxPlayer
    {
        public void Play();
        public void PlayImpact(ShotTrace trace, float arrival);
    }
}
