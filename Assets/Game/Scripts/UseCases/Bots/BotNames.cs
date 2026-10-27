using System;
using System.Collections.Generic;

namespace Bw.UseCases.Bots
{
    public static class BotNames
    {
        public static string Pick(IReadOnlyList<string> names, ICollection<string> taken, Random random)
        {
            var free = new List<string>(names.Count);
            foreach (var name in names)
                if (!taken.Contains(name))
                    free.Add(name);

            if (free.Count == 0)
                throw new InvalidOperationException($"All {names.Count.ToString()} bot names are taken: the room config lists fewer names than bots.");

            return free[random.Next(free.Count)];
        }
    }
}
