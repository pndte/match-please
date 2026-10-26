using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network
{
    public sealed class NicknamePublisher
    {
        public NicknamePublisher(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyCharacter character,
            ICharacterRegistry registry,
            INetEntries entries,
            NetPropertyDeclaration<string> declaration)
        {
            var published = entries.Property(declaration);
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                registry.PlayerByCharacter.ViewForKey(spawnedLifetime, character, (_, player) => published.Value = player.Nickname));
        }
    }
}
