namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IShotTracer
    {
        public ShotRay Aim(float aim);
        public ShotTrace Cast(ShotRay ray);
    }
}
