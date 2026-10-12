using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Network.Extensions
{
    public static class ShotTracerExtensions
    {
        public static void CastPellets(this IShotTracer tracer, ShotRay ray, Hit hit, List<AffectedTarget<IReadonlyMortal, Hit>> affected)
        {
            for (var pellet = 0; pellet < tracer.Pellets; pellet++)
            {
                var struck = tracer.Cast(tracer.Pellet(ray, pellet)).Hit;
                if (struck && struck.collider.TryGetComponent<IHolder<IReadonlyMortal>>(out var mortal))
                    Add(affected, mortal.Value, hit);
            }
        }

        private static void Add(List<AffectedTarget<IReadonlyMortal, Hit>> affected, IReadonlyMortal mortal, Hit hit)
        {
            for (var index = 0; index < affected.Count; index++)
            {
                if (!EqualityComparer<IReadonlyMortal>.Default.Equals(affected[index].Target, mortal))
                    continue;

                affected[index] = new AffectedTarget<IReadonlyMortal, Hit>(mortal, affected[index].Effect.Plus(hit));
                return;
            }

            affected.Add(new AffectedTarget<IReadonlyMortal, Hit>(mortal, hit));
        }
    }
}
