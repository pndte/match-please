namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public readonly struct TriggerControls
    {
        public readonly bool Pulled;
        public readonly float ViewDelay;
        public readonly float PredictedDelay;
        public readonly TriggerPress Press;

        public TriggerControls(
            bool pulled,
            float viewDelay,
            float predictedDelay,
            TriggerPress press)
        {
            Pulled = pulled;
            ViewDelay = viewDelay;
            PredictedDelay = predictedDelay;
            Press = press;
        }
    }
}
