namespace Bw.Entities.Network.Prediction.Events
{
    public interface IPredictionRules<TState, TEffect> where TState : struct where TEffect : struct
    {
        public TState Predict(TState state, TEffect effect);
        public TState Present(TState shown, TState next, ChangeCause<TState, TEffect> cause);
    }
}
