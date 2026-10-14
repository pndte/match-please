using System;

namespace Setup.Launch
{
    public static class LaunchArguments
    {
        public const string Server = "-server";
        public const string Connect = "-connect";
        public const string Room = "-room";
        public const string Owner = "-owner";

        public static bool TryRead(string[] arguments, ushort defaultPort, out LaunchChoice choice)
        {
            for (var index = 0; index < arguments.Length; index++)
            {
                switch (arguments[index])
                {
                    case Server:
                        choice = new LaunchChoice.Server(PortAfter(arguments, index, defaultPort));
                        return true;
                    case Room:
                        choice = new LaunchChoice.Room(PortAfter(arguments, index, defaultPort));
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
                server => $"{Server} {server.Port}",
                client => $"{Connect} {client.Endpoint()}",
                room => $"{Room} {room.Port}");

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
                throw new ArgumentException($"'{Connect}' needs an IPv4 address with an optional port after it, like '{Connect} 26.12.34.56:{defaultPort}'.");

            return client;
        }
    }
}
