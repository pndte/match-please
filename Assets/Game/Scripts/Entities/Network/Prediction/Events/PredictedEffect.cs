namespace Bw.Entities.Network.Prediction.Events
{
    public readonly struct PredictedEffect<TEffect> where TEffect : struct
    {
        public readonly ActionId Action;
        public readonly TEffect Value;

        public PredictedEffect(ActionId action, TEffect value)
        {
            Action = action;
            Value = value;
        }
    }
}
