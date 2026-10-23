using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class HandsHold
    {
        public IReadonlyViewableList<IWeapon> Gripped => _gripped;

        private readonly BwViewableList<IWeapon> _gripped = new();
        private readonly BwViewableList<IHeldBox> _boxes = new();
        private readonly IReadonlyHands _hands;
        private readonly HandsGrip _grip;

        public HandsHold(
            Lifetime lifetime,
            HandsDraw draw,
            IHeldBoxes boxes,
            IReadonlyHands hands,
            HandsGrip grip)
        {
            _hands = hands;
            _grip = grip;

            draw.Drawn.View(lifetime, (drawnLifetime, weapon) =>
                boxes.ByWeapon.ViewForKey(drawnLifetime, weapon, (grippedLifetime, box) => Hold(grippedLifetime, weapon, box)));
        }

        public void Carry()
        {
            for (var index = 0; index < _boxes.Count; index++)
                Place(_boxes[index]);
        }

        private void Hold(Lifetime grippedLifetime, IWeapon weapon, IHeldBox box)
        {
            _grip.Enter(grippedLifetime, box);
            Place(box);
            box.Turned.Advise(grippedLifetime, _ => Place(box));
            _boxes.AddLifetimed(grippedLifetime, box);
            _gripped.AddLifetimed(grippedLifetime, weapon);
        }

        private void Place(IHeldBox box) =>
            _grip.Place(box.PoseIn(_hands.Pose, _grip.Pose));
    }
}
