namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IInputDivergence<TInput>
        where TInput : struct
    {
        public bool Diverged(TInput guessed, TInput relayed);
    }
}
