namespace Bw.UseCases.Movement.Abstractions
{
    public interface IBodyState<TState> where TState : struct
    {
        public BodyState Body { get; }
        public TState WithBody(BodyState body);
    }
}
