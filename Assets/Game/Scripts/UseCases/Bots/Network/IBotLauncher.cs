using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public interface IBotLauncher
    {
        public void Launch(
            Lifetime lifetime,
            IReadonlyCharacter character,
            BotDifficultyConfig difficulty,
            BotPersonality personality);
    }
}
