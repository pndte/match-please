namespace Bw.Entities.Network.Prediction.Events
{
    public interface IEffectRules<TState, TEffect> where TState : struct where TEffect : struct
    {
        public TState Apply(TState state, TEffect effect);
    }
}
