using Bw.Entities.Players;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Match.Network
{
    public readonly struct Kill
    {
        public readonly IPlayer Killer;
        public readonly IPlayer Victim;
        public readonly IReadonlyWeapon Weapon;

        public Kill(IPlayer killer, IPlayer victim, IReadonlyWeapon weapon)
        {
            Killer = killer;
            Victim = victim;
            Weapon = weapon;
        }
    }
}
