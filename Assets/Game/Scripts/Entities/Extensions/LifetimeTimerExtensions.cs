using System;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.Entities.Extensions
{
    public static class LifetimeTimerExtensions
    {
        public static Lifetime CreateNestedFor(this Lifetime lifetime, float seconds)
        {
            var timed = lifetime.CreateNested();
            timed.Lifetime.WhenElapsed(seconds, () => timed.Terminate());
            return timed.Lifetime;
        }

        public static void WhenElapsed(this Lifetime lifetime, float seconds, Action handler)
        {
            if (seconds <= 0f)
            {
                lifetime.TryExecute(handler);
                return;
            }

            var wait = lifetime.CreateNested();
            var end = Time.time + seconds;
            Observable.EveryUpdate(UnityFrameProvider.Update, wait.Lifetime)
                .Where(_ => Time.time >= end)
                .Take(1)
                .Subscribe(_ =>
                {
                    wait.Terminate();
                    handler();
                });
        }
    }
}
