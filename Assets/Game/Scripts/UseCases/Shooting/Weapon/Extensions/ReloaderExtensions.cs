using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class ReloaderExtensions
    {
        public static void AdviseReloadComplete(this IReloader reloader, Lifetime lifetime, Action handler)
        {
            reloader.State.Advise(lifetime, reloadState =>
            {
                if (reloadState != ReloadState.Complete) return;
                
                handler();
            });
        }
    }
}