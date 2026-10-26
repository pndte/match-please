using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Shooting.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Shooting.Network
{
    public sealed class FlashlightPublisher
    {
        public FlashlightPublisher(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyFlashlight flashlight,
            INetEntries entries,
            NetPropertyDeclaration<bool> declaration)
        {
            var published = entries.Property(declaration);
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                flashlight.Lit.Advise(spawnedLifetime, lit => published.Value = lit));
        }
    }
}
