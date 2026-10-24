using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class DrawControlSampler
    {
        private readonly IReadonlyHands _hands;
        private readonly WeaponInventoryConfig _config;

        private DrawControl _latched = DrawControl.Keep;

        public DrawControlSampler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyHands hands,
            WeaponInventoryConfig config)
        {
            _hands = hands;
            _config = config;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => LatchChoice());
                controlledLifetime.OnTermination(() => _latched = DrawControl.Keep);
            });
        }

        public DrawControl Sample()
        {
            if (_hands.Busy)
                return DrawControl.Keep;

            var choice = _latched;
            _latched = DrawControl.Keep;
            return choice;
        }

        private void LatchChoice() //TODO: new input system
        {
            for (var index = 0; index < _config.Slots.Length; index++)
                if (Input.GetKeyDown(_config.Slots[index].Key))
                    _latched = DrawControl.ToSlot(index);

            if (Input.GetKeyDown(_config.LastKey))
                _latched = DrawControl.Last;

            var scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
                _latched = DrawControl.Scrolled(scroll > 0f ? -1 : 1);
        }
    }
}
