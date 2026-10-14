using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Setup.Launch
{
    public static class BackgroundServer
    {
        public static void Open(ushort port)
        {
#if UNITY_EDITOR
            throw new InvalidOperationException("The editor can't open a background server: it is the game's own exe started in batch mode, so a room opens from a built game.");
#else
            using var server = Process.Start(new ProcessStartInfo(Environment.GetCommandLineArgs()[0], ArgumentsFor(port))
            {
                UseShellExecute = false,
                CreateNoWindow = true
            });
#endif
        }

        public static string LogOf(ushort port) =>
            Path.Combine(Application.persistentDataPath, $"Server-{port}.log");

        private static string ArgumentsFor(ushort port)
        {
            using var owner = Process.GetCurrentProcess();
            return $"-batchmode -nographics {LaunchArguments.Of(new LaunchChoice.Server(port))} {LaunchArguments.Owner} {owner.Id} -logFile \"{LogOf(port)}\"";
        }
    }
}
