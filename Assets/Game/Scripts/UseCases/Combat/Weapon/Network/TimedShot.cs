using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public readonly struct TimedShot
    {
        public readonly WeaponShot Shot;
        public readonly ShotTiming Timing;

        public TimedShot(WeaponShot shot, ShotTiming timing)
        {
            Shot = shot;
            Timing = timing;
        }
    }
}
