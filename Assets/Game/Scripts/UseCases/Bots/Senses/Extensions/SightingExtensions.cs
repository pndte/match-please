using System.Collections.Generic;
using UnityEngine;

namespace Bw.UseCases.Bots.Senses.Extensions
{
    public static class SightingExtensions
    {
        public static Sighting Seen(this Sighting sighting, Vector2 position, int reactionTicks) =>
            new(sighting.Character, position, true, sighting.Noticed || sighting.SeenTicks + 1 >= reactionTicks, sighting.SeenTicks + 1, 0);

        public static Sighting Unseen(this Sighting sighting) =>
            new(sighting.Character, sighting.Position, false, sighting.Noticed, 0, sighting.UnseenTicks + 1);

        public static bool TryFindTarget(this IReadOnlyList<Sighting> sightings, Vector2 eye, out Sighting target)
        {
            var found = false;
            target = default;
            for (var index = 0; index < sightings.Count; index++)
            {
                var sighting = sightings[index];
                if (!sighting.Noticed || found && !IsBetter(sighting, target, eye))
                    continue;

                target = sighting;
                found = true;
            }

            return found;
        }

        private static bool IsBetter(Sighting sighting, Sighting best, Vector2 eye)
        {
            if (sighting.Visible != best.Visible)
                return sighting.Visible;

            return sighting.Visible
                ? (sighting.Position - eye).sqrMagnitude < (best.Position - eye).sqrMagnitude
                : sighting.UnseenTicks < best.UnseenTicks;
        }
    }
}
