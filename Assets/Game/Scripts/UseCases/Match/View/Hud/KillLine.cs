using UnityEngine;

namespace Bw.UseCases.Match.View.Hud
{
    public readonly struct KillLine
    {
        public readonly KillName Killer;
        public readonly KillName Victim;
        public readonly Sprite WeaponIcon;

        public KillLine(KillName killer, KillName victim, Sprite weaponIcon)
        {
            Killer = killer;
            Victim = victim;
            WeaponIcon = weaponIcon;
        }
    }

    public readonly struct KillName
    {
        public readonly string Nickname;
        public readonly bool Mine;

        public KillName(string nickname, bool mine)
        {
            Nickname = nickname;
            Mine = mine;
        }
    }
}
