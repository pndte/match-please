namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct WeaponShot
    {
        public readonly float Aim;
        public readonly float ViewDelay;

        public WeaponShot(float aim, float viewDelay)
        {
            Aim = aim;
            ViewDelay = viewDelay;
        }
    }
}
