namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IShotTracer
    {
        public ShotTrace Trace(float aim);
    }
}
