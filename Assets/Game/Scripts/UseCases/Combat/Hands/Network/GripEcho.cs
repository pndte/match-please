using Bw.Entities.Extensions;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class GripEcho
    {
        private readonly IReadonlyHands _hands;
        private readonly IHeldBoxes _boxes;
        private readonly HandsGrip _echo;
        private readonly IInterpolatedLifetimes _lifetimes;
        private readonly SequentialLifetimes _echoes;

        public GripEcho(
            Lifetime lifetime,
            HandsHold hold,
            IReadonlyHands hands,
            IHeldBoxes boxes,
            HandsGrip echo,
            IInterpolatedLifetimes lifetimes)
        {
            _hands = hands;
            _boxes = boxes;
            _echo = echo;
            _lifetimes = lifetimes;
            _echoes = new SequentialLifetimes(lifetime);

            hold.Gripped.View(lifetime, (grippedLifetime, weapon) =>
            {
                _echoes.TerminateCurrent();
                grippedLifetime.OnTermination(() =>
                {
                    if (lifetime.IsAlive && _boxes.ByWeapon.TryGetValue(weapon, out var box))
                        Echo(weapon, box);
                });
            });
        }

        private void Echo(IWeapon weapon, IHeldBox box)
        {
            var echo = new LifetimeDefinition(_lifetimes.CreateUntilShown(_echoes.Next()));
            _boxes.ByWeapon.AdviseAddRemove(echo.Lifetime, (kind, released, _) =>
            {
                if (kind == AddRemove.Remove && released == weapon)
                    echo.Terminate();
            });
            _echo.Enter(echo.Lifetime, box);
            _echo.Place(_hands.Grip);
        }
    }
}
