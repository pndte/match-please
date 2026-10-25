namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IInputDivergence<TInput>
        where TInput : struct
    {
        public bool Differs(TInput guessed, TInput relayed);
    }
}
