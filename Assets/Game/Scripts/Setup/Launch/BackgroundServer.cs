using System.Diagnostics;
using System.IO;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Setup.Launch
{
    public static class BackgroundServer
    {
        public static bool Available => File.Exists(Executable);

        public static string Executable =>
#if UNITY_EDITOR
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "WindowsClient", "MatchPlease.exe"));
#else
            System.Environment.GetCommandLineArgs()[0];
#endif

        public static void Open(Lifetime lifetime, ushort port)
        {
            var server = Process.Start(new ProcessStartInfo(Executable, ArgumentsFor(port))
            {
                UseShellExecute = false,
                CreateNoWindow = true
            });
            lifetime.OnTermination(() => Stop(server));
        }

        public static string LogOf(ushort port) =>
            Path.Combine(Application.persistentDataPath, $"Server-{port}.log");

        private static void Stop(Process server)
        {
            using (server)
            {
                if (!server.HasExited)
                    server.Kill();
            }
        }

        private static string ArgumentsFor(ushort port)
        {
            using var owner = Process.GetCurrentProcess();
            return $"-batchmode -nographics {LaunchArguments.Of(new LaunchChoice.Server(port))} {LaunchArguments.Owner} {owner.Id} -logFile \"{LogOf(port)}\"";
        }
    }
}
