using Bw.UseCases.Shooting.Weapon;

namespace Bw.UseCases.Match
{
    public readonly struct KillNote
    {
        public readonly KillParty Killer;
        public readonly KillParty Victim;
        public readonly WeaponDescription Weapon;

        public KillNote(KillParty killer, KillParty victim, WeaponDescription weapon)
        {
            Killer = killer;
            Victim = victim;
            Weapon = weapon;
        }
    }

    public readonly struct KillParty
    {
        public readonly string Nickname;
        public readonly bool IsMine;

        public KillParty(string nickname, bool isMine)
        {
            Nickname = nickname;
            IsMine = isMine;
        }
    }
}
