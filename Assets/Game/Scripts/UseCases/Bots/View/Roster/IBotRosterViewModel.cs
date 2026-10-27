using System.Collections.Generic;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Bots.View.Roster
{
    public interface IBotRosterViewModel
    {
        public IReadonlyProperty<IReadOnlyList<BotRow>> Rows { get; }
        public IReadonlyProperty<bool> Full { get; }

        public void Add();
        public void Remove(int row);
        public void CycleDifficulty(int row);
        public void CyclePersonality(int row);
    }
}
