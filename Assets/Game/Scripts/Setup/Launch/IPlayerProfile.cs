using System;

namespace Setup.Launch
{
    public interface IPlayerProfile
    {
        public string Nickname { get; }
        public Guid Token { get; }
        public int CharacterKey { get; }
        public ushort RoomPort { get; }
        public bool TryGetLastServer(out LaunchChoice.Client server);
        public void SaveNickname(string nickname);
        public void SaveCharacterKey(int characterKey);
        public void SaveRoomPort(ushort port);
        public void SaveLastServer(LaunchChoice.Client server);
    }
}
