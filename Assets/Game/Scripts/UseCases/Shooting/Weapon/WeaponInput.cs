namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct WeaponInput
    {
        public readonly float Aim;
        public readonly bool Trigger;
        public readonly bool Reload;
        public readonly float ViewDelay;
        public readonly float PredictedDelay;
        public readonly TriggerPress Press;

        public WeaponInput(
            float aim,
            bool trigger,
            bool reload,
            float viewDelay,
            float predictedDelay,
            TriggerPress press)
        {
            Aim = aim;
            Trigger = trigger;
            Reload = reload;
            ViewDelay = viewDelay;
            PredictedDelay = predictedDelay;
            Press = press;
        }
    }
}
