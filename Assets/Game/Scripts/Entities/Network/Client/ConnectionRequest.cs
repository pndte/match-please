namespace Bw.Entities.Network
{
    public readonly struct ConnectionRequest
    {
        public readonly string Nickname;
        public readonly int CharacterKey;

        public ConnectionRequest(string nickname, int characterKey)
        {
            Nickname = nickname;
            CharacterKey = characterKey;
        }
    }
}
