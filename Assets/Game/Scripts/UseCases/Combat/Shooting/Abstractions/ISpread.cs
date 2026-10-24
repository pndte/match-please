namespace Bw.UseCases.Combat.Shooting.Abstractions
{
    public interface ISpread
    {
        public float ThrowOf(float aim, float roll);
        public float Returned(float deflection, float seconds);
    }
}
