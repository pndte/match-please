namespace Bw.Entities.Network.Prediction.Events
{
    public interface IOutcomePrediction<TTarget, TEffect> where TTarget : class where TEffect : struct
    {
        public void Predict(int tick, TTarget target, TEffect effect);
    }
}
