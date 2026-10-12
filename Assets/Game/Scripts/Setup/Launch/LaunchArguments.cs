using System;

namespace Setup.Launch
{
    public static class LaunchArguments
    {
        public const string Server = "-server";
        public const string Connect = "-connect";

        public static bool TryRead(string[] arguments, ushort defaultPort, out LaunchChoice choice)
        {
            for (var index = 0; index < arguments.Length; index++)
            {
                if (arguments[index] == Server)
                {
                    choice = new LaunchChoice.Server(defaultPort);
                    return true;
                }

                if (arguments[index] != Connect)
                    continue;

                if (index + 1 >= arguments.Length || !LaunchChoice.Client.TryParse(arguments[index + 1], defaultPort, out var client))
                    throw new ArgumentException($"'{Connect}' needs an IPv4 address with an optional port after it, like '{Connect} 26.12.34.56:{defaultPort}'.");

                choice = client;
                return true;
            }

            choice = null;
            return false;
        }
    }
}
