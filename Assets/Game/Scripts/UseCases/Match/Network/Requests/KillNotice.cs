namespace Bw.UseCases.Match.Network.Requests
{
    public readonly struct KillNotice
    {
        public readonly PlayerTag Killer;
        public readonly PlayerTag Victim;
        public readonly int WeaponKey;

        public KillNotice(PlayerTag killer, PlayerTag victim, int weaponKey)
        {
            Killer = killer;
            Victim = victim;
            WeaponKey = weaponKey;
        }
    }

    public readonly struct PlayerTag
    {
        public readonly ulong ClientId;
        public readonly string Nickname;

        public PlayerTag(ulong clientId, string nickname)
        {
            ClientId = clientId;
            Nickname = nickname;
        }
    }
}
