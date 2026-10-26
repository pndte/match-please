using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Bw.EditorTools.Build
{
    public static class GameBuilder
    {
        private const string Menu = "Tools/Build/";
        private const string WindowsClientMenu = Menu + "Windows Client";
        private const string LinuxServerMenu = Menu + "Linux Dedicated Server";
        private const string LocalBuilds = "Builds";
        private const string OutputArgument = "-buildOutput";
        private const string VersionArgument = "-buildVersion";

        public static void WindowsClient() =>
            BuildFromCommandLine(GameBuildTarget.WindowsClient);

        public static void LinuxServer() =>
            BuildFromCommandLine(GameBuildTarget.LinuxServer);

        [MenuItem(WindowsClientMenu)]
        public static void BuildWindowsClient() =>
            Build(GameBuildTarget.WindowsClient, Path.Combine(LocalBuilds, GameBuildTarget.WindowsClient.Name));

        [MenuItem(LinuxServerMenu)]
        public static void BuildLinuxServer() =>
            Build(GameBuildTarget.LinuxServer, Path.Combine(LocalBuilds, GameBuildTarget.LinuxServer.Name));

        [MenuItem(WindowsClientMenu, true)]
        [MenuItem(LinuxServerMenu, true)]
        public static bool IsBuildAvailable() =>
            !EditorApplication.isPlayingOrWillChangePlaymode;

        private static void BuildFromCommandLine(GameBuildTarget target)
        {
            var output = ArgumentAfter(OutputArgument);
            var version = PlayerSettings.bundleVersion;
            if (TryGetArgumentAfter(VersionArgument, out var requested))
                PlayerSettings.bundleVersion = requested;

            BuildReport report;
            try
            {
                report = Build(target, output);
            }
            finally
            {
                PlayerSettings.bundleVersion = version;
            }

            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static BuildReport Build(GameBuildTarget target, string folder)
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0)
                throw new InvalidOperationException("Build Settings list no enabled scene: there is nothing to build.");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(folder, target.Executable),
                target = target.Target,
                subtarget = (int)target.Subtarget,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.StrictMode,
            });
            var summary = report.summary;
            Debug.Log($"[GameBuilder] {target.Name} {PlayerSettings.bundleVersion}: {summary.result}, {summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:0} s, {summary.totalErrors} errors, {summary.outputPath}");
            return report;
        }

        private static string ArgumentAfter(string name)
        {
            if (!TryGetArgumentAfter(name, out var value))
                throw new ArgumentException($"The build needs '{name} <value>' on Unity's command line.");

            return value;
        }

        private static bool TryGetArgumentAfter(string name, out string value)
        {
            var arguments = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(arguments, name);
            if (index < 0 || index + 1 >= arguments.Length)
            {
                value = default;
                return false;
            }

            value = arguments[index + 1];
            return true;
        }
    }
}
