namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct WeaponShot
    {
        public readonly float Aim;
        public readonly float ViewDelay;
        public readonly float PredictedDelay;

        public WeaponShot(float aim, float viewDelay, float predictedDelay)
        {
            Aim = aim;
            ViewDelay = viewDelay;
            PredictedDelay = predictedDelay;
        }
    }
}
