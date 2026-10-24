using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Shooting.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Shooting.Network
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
