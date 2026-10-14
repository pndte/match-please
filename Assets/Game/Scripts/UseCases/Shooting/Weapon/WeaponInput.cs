namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct WeaponInput
    {
        public readonly float Aim;
        public readonly bool Trigger;
        public readonly bool Reload;
        public readonly float ViewDelay;
        public readonly float PredictedDelay;

        public WeaponInput(
            float aim,
            bool trigger,
            bool reload,
            float viewDelay,
            float predictedDelay)
        {
            Aim = aim;
            Trigger = trigger;
            Reload = reload;
            ViewDelay = viewDelay;
            PredictedDelay = predictedDelay;
        }
    }
}
