namespace Bw.Entities.Network.Prediction.Events
{
    public interface IPredictionTarget<TEffect> where TEffect : struct
    {
        public void Show(PredictedEffect<TEffect> effect);
        public void Confirm(ActionId action, TEffect actual);
        public void Reject(ActionId action);
    }
}
