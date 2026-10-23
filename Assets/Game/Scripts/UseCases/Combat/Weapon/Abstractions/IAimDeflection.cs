namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IReadonlyAimDeflection
    {
        public float Current { get; }
    }

    public interface IAimDeflection : IReadonlyAimDeflection
    {
        public void Kick(float degrees);
    }
}
