namespace Bw.UseCases.Combat.Abstractions.Weapons
{
    public readonly struct WeaponInput
    {
        public readonly float Aim;
        public readonly bool Pulled;
        public readonly bool ReloadPressed;
        public readonly bool Guarded;

        public WeaponInput(
            float aim,
            bool pulled,
            bool reloadPressed,
            bool guarded)
        {
            Aim = aim;
            Pulled = pulled;
            ReloadPressed = reloadPressed;
            Guarded = guarded;
        }
    }
}
