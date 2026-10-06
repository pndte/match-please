namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct WeaponInput
    {
        public readonly float Aim;
        public readonly bool Trigger;
        public readonly bool Reload;

        public WeaponInput(float aim, bool trigger, bool reload)
        {
            Aim = aim;
            Trigger = trigger;
            Reload = reload;
        }
    }
}
