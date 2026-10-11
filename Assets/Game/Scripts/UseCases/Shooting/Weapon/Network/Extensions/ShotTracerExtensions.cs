using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Network.Extensions
{
    public static class ShotTracerExtensions
    {
        public static void CastPellets(this IShotTracer tracer, ShotRay ray, Hit hit, List<AffectedTarget<IReadonlyCharacter, Hit>> affected)
        {
            for (var pellet = 0; pellet < tracer.Pellets; pellet++)
            {
                var struck = tracer.Cast(tracer.Pellet(ray, pellet)).Hit;
                if (struck && struck.collider.TryGetComponent<IHolder<IReadonlyCharacter>>(out var character))
                    Add(affected, character.Value, hit);
            }
        }

        private static void Add(List<AffectedTarget<IReadonlyCharacter, Hit>> affected, IReadonlyCharacter character, Hit hit)
        {
            for (var index = 0; index < affected.Count; index++)
            {
                if (!EqualityComparer<IReadonlyCharacter>.Default.Equals(affected[index].Target, character))
                    continue;

                affected[index] = new AffectedTarget<IReadonlyCharacter, Hit>(character, affected[index].Effect.Plus(hit));
                return;
            }

            affected.Add(new AffectedTarget<IReadonlyCharacter, Hit>(character, hit));
        }
    }
}
