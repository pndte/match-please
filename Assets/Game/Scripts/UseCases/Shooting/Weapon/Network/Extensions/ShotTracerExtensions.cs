using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Network.Extensions
{
    public static class ShotTracerExtensions
    {
        public static void CastPellets(
            this IShotTracer tracer,
            ShotRay ray,
            Hit hit,
            List<AffectedTarget<IReadonlyMortal, Impact>> struck)
        {
            for (var pellet = 0; pellet < tracer.Pellets; pellet++)
            {
                var trace = tracer.Cast(tracer.Pellet(ray, pellet));
                if (trace.IsStruck && trace.Shape.TryGetMortal(out var mortal))
                    struck.Add(new AffectedTarget<IReadonlyMortal, Impact>(mortal, new Impact(hit, trace.Shape.ContactAt(trace.To))));
            }
        }

        public static void GroupByTarget(this List<AffectedTarget<IReadonlyMortal, Impact>> struck, List<AffectedTarget<IReadonlyMortal, Impacts>> affected)
        {
            for (var index = 0; index < struck.Count; index++)
            {
                var target = struck[index].Target;
                if (IsListed(affected, target))
                    continue;

                var impacts = new Impact[CountOf(struck, target, index)];
                var filled = 0;
                for (var each = index; each < struck.Count; each++)
                    if (Same(struck[each].Target, target))
                        impacts[filled++] = struck[each].Effect;

                affected.Add(new AffectedTarget<IReadonlyMortal, Impacts>(target, new Impacts(impacts)));
            }
        }

        private static int CountOf(List<AffectedTarget<IReadonlyMortal, Impact>> struck, IReadonlyMortal target, int from)
        {
            var count = 0;
            for (var index = from; index < struck.Count; index++)
                if (Same(struck[index].Target, target))
                    count++;

            return count;
        }

        private static bool IsListed(List<AffectedTarget<IReadonlyMortal, Impacts>> affected, IReadonlyMortal target)
        {
            for (var index = 0; index < affected.Count; index++)
                if (Same(affected[index].Target, target))
                    return true;

            return false;
        }

        private static bool Same(IReadonlyMortal first, IReadonlyMortal second) =>
            EqualityComparer<IReadonlyMortal>.Default.Equals(first, second);
    }
}
