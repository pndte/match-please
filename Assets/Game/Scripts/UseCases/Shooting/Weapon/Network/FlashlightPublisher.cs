using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
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
            var published = entries.Get(declaration);
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                flashlight.Lit.Advise(spawnedLifetime, lit => published.Value = lit));
        }
    }
}
