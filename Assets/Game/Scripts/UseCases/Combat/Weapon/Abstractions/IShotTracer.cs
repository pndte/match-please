namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IShotTracer
    {
        public int Pellets { get; }
        public ShotRay Aim(float aim);
        public ShotRay Pellet(ShotRay ray, int pellet);
        public ShotTrace Cast(ShotRay ray);
    }
}
