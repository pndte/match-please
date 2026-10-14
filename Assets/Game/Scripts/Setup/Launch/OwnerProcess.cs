using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Setup.Launch
{
    public static class OwnerProcess
    {
        private static readonly TimeSpan Poll = TimeSpan.FromSeconds(1);

        public static async UniTaskVoid QuitWhenGone(int ownerId)
        {
            while (IsAlive(ownerId))
                await UniTask.Delay(Poll, DelayType.Realtime);

            Debug.Log($"[OwnerProcess] Process {ownerId}, whose window owns this server, is gone: quitting.");
            Application.Quit();
        }

        private static bool IsAlive(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                return !process.HasExited;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
