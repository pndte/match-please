using UnityEditor;

namespace Bw.EditorTools.Build
{
    public readonly struct GameBuildTarget
    {
        public static readonly GameBuildTarget WindowsClient = new("WindowsClient", BuildTarget.StandaloneWindows64, StandaloneBuildSubtarget.Player, "MatchPlease.exe");
        public static readonly GameBuildTarget LinuxServer = new("LinuxServer", BuildTarget.StandaloneLinux64, StandaloneBuildSubtarget.Server, "MatchPleaseServer.x86_64");

        public readonly string Name;
        public readonly BuildTarget Target;
        public readonly StandaloneBuildSubtarget Subtarget;
        public readonly string Executable;

        public GameBuildTarget(
            string name,
            BuildTarget target,
            StandaloneBuildSubtarget subtarget,
            string executable)
        {
            Name = name;
            Target = target;
            Subtarget = subtarget;
            Executable = executable;
        }
    }
}
