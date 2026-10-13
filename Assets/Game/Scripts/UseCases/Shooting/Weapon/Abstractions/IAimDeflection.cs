namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IAimDeflection
    {
        public float Current { get; }
        public void Kick(float degrees);
    }
}
