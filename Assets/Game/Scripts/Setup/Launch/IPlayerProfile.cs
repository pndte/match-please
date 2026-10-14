namespace Setup.Launch
{
    public interface IPlayerProfile
    {
        public string Nickname { get; }
        public string CharacterKey { get; }
        public ushort RoomPort { get; }
        public bool TryGetLastServer(out LaunchChoice.Client server);
        public void SaveNickname(string nickname);
        public void SaveCharacterKey(string characterKey);
        public void SaveRoomPort(ushort port);
        public void SaveLastServer(LaunchChoice.Client server);
    }
}
