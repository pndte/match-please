namespace Bw.UseCases.Combat.Weapon.Network
{
    public readonly struct ShotTiming
    {
        public readonly float ViewDelay;
        public readonly float PredictedDelay;
        public readonly int LateTicks;

        public ShotTiming(float viewDelay, float predictedDelay, int lateTicks)
        {
            ViewDelay = viewDelay;
            PredictedDelay = predictedDelay;
            LateTicks = lateTicks;
        }
    }
}
