using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Characters
{
    public sealed class InHandsRegistration
    {
        public InHandsRegistration(
            Lifetime lifetime,
            IReadonlyCharacter character,
            HandsDraw draw,
            IWeaponsInHands inHands)
        {
            draw.Drawn.View(lifetime, (drawnLifetime, weapon) =>
                inHands.ByWeapon.AddLifetimed(drawnLifetime, weapon, character));
        }
    }
}
