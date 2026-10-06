using Bw.UseCases.Shooting.Weapon;

namespace Bw.UseCases.Shooting.View.Audio.Abstractions
{
    public interface IShotSfxPlayer
    {
        public void Play(ShotTrace trace, float arrival);
    }
}
