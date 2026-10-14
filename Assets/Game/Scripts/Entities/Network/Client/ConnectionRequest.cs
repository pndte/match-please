namespace Bw.Entities.Network
{
    public readonly struct ConnectionRequest
    {
        public readonly string Nickname;
        public readonly string CharacterKey;

        public ConnectionRequest(string nickname, string characterKey)
        {
            Nickname = nickname;
            CharacterKey = characterKey;
        }
    }
}
