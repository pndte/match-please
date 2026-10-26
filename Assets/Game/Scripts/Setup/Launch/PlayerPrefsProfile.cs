using System;
using Bw.Entities.Network;
using UnityEngine;

namespace Setup.Launch
{
    public sealed class PlayerPrefsProfile : IPlayerProfile
    {
        private const string NicknamePref = "Launch.PlayerName"; //TODO: config 
        private const string CharacterPref = "Launch.CharacterKey";
        private const string RoomPortPref = "Launch.RoomPort";
        private const string LastServerPref = "Launch.ServerAddress";
        private const string TokenPref = "Launch.PlayerToken";
        private const string TokenFormat = "N";

        public string Nickname => PlayerPrefs.GetString(NicknamePref, string.Empty);
        public int CharacterKey => PlayerPrefs.GetInt(CharacterPref, 0);

        public Guid Token =>
            Guid.TryParseExact(PlayerPrefs.GetString(TokenPref, string.Empty), TokenFormat, out var token) ? token : SavedNewToken();

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

        private static Guid SavedNewToken()
        {
            var token = Guid.NewGuid();
            Save(TokenPref, token.ToString(TokenFormat));
            return token;
        }

        private static void Save(string pref, string value)
        {
            PlayerPrefs.SetString(pref, value);
            PlayerPrefs.Save();
        }
    }
}
