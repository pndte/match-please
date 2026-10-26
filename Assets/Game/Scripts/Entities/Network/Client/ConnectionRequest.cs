using System;

namespace Bw.Entities.Network
{
    public readonly struct ConnectionRequest
    {
        public readonly string Nickname;
        public readonly int CharacterKey;
        public readonly Guid Token;

        public ConnectionRequest(string nickname, int characterKey, Guid token)
        {
            Nickname = nickname;
            CharacterKey = characterKey;
            Token = token;
        }
    }
}
