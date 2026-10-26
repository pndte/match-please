using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Characters
{
    public sealed class InHandsRegistrar
    {
        public InHandsRegistrar(
            Lifetime lifetime,
            IReadonlyCharacter character,
            DrawnWeapons drawnWeapons,
            IWeaponsInHands inHands)
        {
            drawnWeapons.Drawn.View(lifetime, (drawnLifetime, weapon) =>
                inHands.Add(drawnLifetime, weapon, character));
        }
    }
}
