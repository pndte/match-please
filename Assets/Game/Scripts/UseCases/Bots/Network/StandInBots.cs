using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public sealed class StandInBots
    {
        public StandInBots(
            Lifetime lifetime,
            IReadonlyDepartedPlayers departed,
            ICharacterRegistry registry,
            IBotLauncher launcher,
            BotsConfig config)
        {
            departed.ByPlayer.View(lifetime, (departedLifetime, player, _) =>
                registry.PlayerByCharacter.View(departedLifetime, (characterLifetime, character, owner) =>
                {
                    if (owner == player)
                        character.Mortal.State.WhenAlive(characterLifetime, aliveLifetime =>
                            launcher.Launch(aliveLifetime, character, config.StandIn, config.StandInPersonality));
                }));
        }
    }
}
