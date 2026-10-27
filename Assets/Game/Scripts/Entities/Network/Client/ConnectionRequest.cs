using System;

namespace Bw.Entities.Network
{
    public readonly struct ConnectionRequest
    {
        public readonly string Nickname;
        public readonly int CharacterKey;
        public readonly Guid Token;
        public readonly Guid HostKey;

        public ConnectionRequest(
            string nickname,
            int characterKey,
            Guid token,
            Guid hostKey)
        {
            Nickname = nickname;
            CharacterKey = characterKey;
            Token = token;
            HostKey = hostKey;
        }
    }
}
