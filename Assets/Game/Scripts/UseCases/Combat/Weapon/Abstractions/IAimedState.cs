namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IAimedState<TState> where TState : struct
    {
        public TState Turned(float degrees);
    }
}
