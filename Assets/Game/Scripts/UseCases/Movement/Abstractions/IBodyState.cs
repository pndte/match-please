namespace Bw.UseCases.Movement.Abstractions
{
    public interface IBodyState<TState> : IPlacedState<TState> where TState : struct
    {
        public BodyState Body { get; }
        public TState WithBody(BodyState body);
    }
}
