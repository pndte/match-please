using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public readonly struct TimedShot
    {
        public readonly WeaponShot Shot;
        public readonly ShotTiming Timing;
        public readonly IReadonlyCharacter Shooter;

        public TimedShot(WeaponShot shot, ShotTiming timing, IReadonlyCharacter shooter)
        {
            Shot = shot;
            Timing = timing;
            Shooter = shooter;
        }
    }
}
