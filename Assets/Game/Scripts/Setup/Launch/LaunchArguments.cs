using System;
using System.Collections.Generic;
using System.Globalization;
using Bw.Entities.Network;

namespace Setup.Launch
{
    public static class LaunchArguments
    {
        public const string Server = "-server";
        public const string Connect = "-connect";
        public const string Room = "-room";
        public const string Owner = "-owner";
        public const string Level = "-level";
        public const string HostKey = "-host-key";
        public const string Bots = "-bots";

        private const char BotSeparator = ';';
        private const char FieldSeparator = '|';
        private const string KeyFormat = "N";

        public static bool TryRead(
            string[] arguments,
            ushort defaultPort,
            RoomSettings defaultRoom,
            out LaunchChoice choice)
        {
            for (var index = 0; index < arguments.Length; index++)
            {
                switch (arguments[index])
                {
                    case Server:
                        choice = new LaunchChoice.Server(PortAfter(arguments, index, defaultPort), RoomOf(arguments, defaultRoom));
                        return true;
                    case Room:
                        choice = new LaunchChoice.Room(PortAfter(arguments, index, defaultPort), RoomOf(arguments, defaultRoom));
                        return true;
                    case Connect:
                        choice = ClientAfter(arguments, index, defaultPort);
                        return true;
                }
            }

            choice = null;
            return false;
        }

        public static bool TryReadOwner(string[] arguments, out int ownerId)
        {
            ownerId = 0;
            var index = Array.IndexOf(arguments, Owner);
            if (index < 0)
                return false;

            if (index + 1 >= arguments.Length || !int.TryParse(arguments[index + 1], out ownerId) || ownerId <= 0)
                throw new ArgumentException($"'{Owner}' needs the id of the process whose window owns this server after it, like '{Owner} 1234'.");

            return true;
        }

        public static string Of(LaunchChoice choice) =>
            choice.Switch(
                server => $"{Server} {server.Port} {RoomArguments(server.Settings)}",
                client => $"{Connect} {client.Endpoint()}",
                room => $"{Room} {room.Port} {RoomArguments(room.Settings)}");

        private static string RoomArguments(RoomSettings room) =>
            $"{Level} {room.Level} {HostKey} {room.HostKey.ToString(KeyFormat)} {Bots} \"{BotsText(room.Bots)}\"";

        private static string BotsText(IReadOnlyList<RoomBot> bots)
        {
            var texts = new string[bots.Count];
            for (var index = 0; index < bots.Count; index++)
            {
                var bot = bots[index];
                texts[index] = bot.Nickname + FieldSeparator + bot.DifficultyKey.ToString(CultureInfo.InvariantCulture) + FieldSeparator + bot.PersonalityKey.ToString(CultureInfo.InvariantCulture);
            }

            return string.Join(BotSeparator.ToString(), texts);
        }

        private static RoomSettings RoomOf(string[] arguments, RoomSettings defaultRoom) =>
            new(
                TryReadValue(arguments, Level, out var level) ? level : defaultRoom.Level,
                TryReadValue(arguments, HostKey, out var hostKey) ? KeyOf(hostKey) : defaultRoom.HostKey,
                TryReadValue(arguments, Bots, out var bots) ? BotsOf(bots) : defaultRoom.Bots);

        private static bool TryReadValue(string[] arguments, string name, out string value)
        {
            value = null;
            var index = Array.IndexOf(arguments, name);
            if (index < 0)
                return false;

            if (index + 1 >= arguments.Length)
                throw new ArgumentException($"'{name}' needs a value after it.");

            value = arguments[index + 1];
            return true;
        }

        private static Guid KeyOf(string text) =>
            Guid.TryParseExact(text, KeyFormat, out var key)
                ? key
                : throw new ArgumentException($"'{HostKey}' needs a key of 32 hex digits after it, not '{text}'.");

        private static IReadOnlyList<RoomBot> BotsOf(string text)
        {
            if (text.Length == 0)
                return Array.Empty<RoomBot>();

            var parts = text.Split(BotSeparator);
            var bots = new RoomBot[parts.Length];
            for (var index = 0; index < parts.Length; index++)
            {
                var fields = parts[index].Split(FieldSeparator);
                if (fields.Length != 3
                    || !int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var difficultyKey)
                    || !int.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out var personalityKey))
                    throw new ArgumentException($"'{Bots}' takes bots as 'nickname|difficulty key|personality key' joined by ';', not '{parts[index]}'.");

                bots[index] = new RoomBot(fields[0], difficultyKey, personalityKey);
            }

            return bots;
        }

        private static ushort PortAfter(string[] arguments, int index, ushort defaultPort)
        {
            if (index + 1 >= arguments.Length || arguments[index + 1].StartsWith("-", StringComparison.Ordinal))
                return defaultPort;

            if (!ushort.TryParse(arguments[index + 1], out var port) || port == 0)
                throw new ArgumentException($"'{arguments[index]}' takes an optional port from 1 to 65535 after it, like '{arguments[index]} {defaultPort}'.");

            return port;
        }

        private static LaunchChoice.Client ClientAfter(string[] arguments, int index, ushort defaultPort)
        {
            if (index + 1 >= arguments.Length || !LaunchChoice.Client.TryParse(arguments[index + 1], defaultPort, out var client))
                throw new ArgumentException($"'{Connect}' needs an IPv4 address with an optional port after it, like '{Connect} 192.168.1.5:{defaultPort}'.");

            return client;
        }
    }
}
