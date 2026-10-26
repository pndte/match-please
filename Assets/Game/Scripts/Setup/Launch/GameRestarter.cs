using System;
using System.Diagnostics;
using UnityEngine;

namespace Setup.Launch
{
    public static class GameRestarter
    {
        public static void ReturnToMenu() => //TODO: в меню возвращаемся перезапуском процесса, а в редакторе выходом из Play: RuntimeSettings принимает роль пира один раз и ProjectContext собран под неё, поэтому внутри того же процесса меню заново не открыть
            Restart(string.Empty);

        public static void Relaunch(LaunchChoice choice) =>
            Restart(LaunchArguments.Of(choice));

        private static void Restart(string arguments)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Process.Start(Environment.GetCommandLineArgs()[0], arguments);
            Application.Quit();
#endif
        }
    }
}
