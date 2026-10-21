using Bw.Entities.Players;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public interface IBotLauncher
    {
        public void Launch(
            Lifetime lifetime,
            IPlayer player,
            IReadonlyMortal character,
            BotDifficultyConfig difficulty);
    }
}
