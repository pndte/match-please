using Bw.Entities.Network;
using UnityEngine;

namespace Setup.Launch
{
    public sealed class PlayerPrefsProfile : IPlayerProfile
    {
        private const string NicknamePref = "Launch.PlayerName";
        private const string CharacterPref = "Launch.CharacterKey";
        private const string RoomPortPref = "Launch.RoomPort";
        private const string LastServerPref = "Launch.ServerAddress";

        public string Nickname => PlayerPrefs.GetString(NicknamePref, string.Empty);
        public int CharacterKey => PlayerPrefs.GetInt(CharacterPref, 0);

        public ushort RoomPort
        {
            get
            {
                var saved = PlayerPrefs.GetInt(RoomPortPref, _connection.Port);
                return saved is > 0 and <= ushort.MaxValue ? (ushort)saved : _connection.Port;
            }
        }

        private readonly ConnectionConfig _connection;

        public PlayerPrefsProfile(ConnectionConfig connection)
        {
            _connection = connection;
        }

        public bool TryGetLastServer(out LaunchChoice.Client server) =>
            LaunchChoice.Client.TryParse(PlayerPrefs.GetString(LastServerPref, string.Empty), _connection.Port, out server);

        public void SaveNickname(string nickname) =>
            Save(NicknamePref, nickname);

        public void SaveCharacterKey(int characterKey)
        {
            PlayerPrefs.SetInt(CharacterPref, characterKey);
            PlayerPrefs.Save();
        }

        public void SaveRoomPort(ushort port)
        {
            PlayerPrefs.SetInt(RoomPortPref, port);
            PlayerPrefs.Save();
        }

        public void SaveLastServer(LaunchChoice.Client server) =>
            Save(LastServerPref, server.Endpoint());

        private static void Save(string pref, string value)
        {
            PlayerPrefs.SetString(pref, value);
            PlayerPrefs.Save();
        }
    }
}
