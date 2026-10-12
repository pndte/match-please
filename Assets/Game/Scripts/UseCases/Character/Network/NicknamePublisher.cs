using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network
{
    public sealed class NicknamePublisher
    {
        public NicknamePublisher(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyMortal character,
            ICharacterRegistry registry,
            IViewableProperty<string> published)
        {
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                registry.PlayerByCharacter.ViewForKey(spawnedLifetime, character, (_, player) => published.Value = player.Nickname));
        }
    }
}
