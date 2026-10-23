namespace Bw.UseCases.Combat.Abstractions.Weapons
{
    public readonly struct WeaponShot
    {
        public readonly float Aim;

        public WeaponShot(float aim)
        {
            Aim = aim;
        }
    }
}
