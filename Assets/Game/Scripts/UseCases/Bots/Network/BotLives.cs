using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotLives
    {
        public BotLives(
            Lifetime lifetime,
            ICharacterRegistry registry,
            IBotRoster roster,
            IBotLauncher launcher)
        {
            roster.Bots.View(lifetime, (botLifetime, bot, entry) =>
                registry.PlayerByCharacter.View(botLifetime, (characterLifetime, character, player) =>
                {
                    if (player == bot)
                        character.State.WhenAlive(characterLifetime, aliveLifetime =>
                            launcher.Launch(aliveLifetime, bot, character, entry.Difficulty));
                }));
        }
    }
}
