using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class FlashlightReceiver
    {
        public FlashlightReceiver(
            Lifetime lifetime,
            INetEntries entries,
            NetPropertyDeclaration<bool> declaration,
            IFlashlight flashlight) =>
            entries.Get(declaration).Advise(lifetime, flashlight.Apply);
    }
}
