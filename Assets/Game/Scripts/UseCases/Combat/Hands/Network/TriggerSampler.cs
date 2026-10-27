using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class TriggerSampler //TODO: спуск читается с мыши напрямую: клик по окну ботов (B) у хоста тоже стреляет
    {
        private TriggerPress LastPress =>
            new(_pressNumber, (byte)Mathf.Min(_ticks.Current - _pressTick, byte.MaxValue), _pressViewDelay, _pressPredictedDelay, 0);

        private bool HoldCounted =>
            _character.Hands.TryFindInHands(out var weapon) && !_character.Inventory.RulesOf(weapon).PressOnly;

        private readonly IReadonlyCharacter _character;
        private readonly GuardKey _guard;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolationTicks;
        private readonly TriggerLatch _trigger = new();

        private double _triggerViewTick;
        private double _triggerPredictedTick;
        private byte _pressNumber;
        private int _pressTick;
        private float _pressViewDelay;
        private float _pressPredictedDelay;

        public TriggerSampler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyCharacter character,
            GuardKey guard,
            INetworkTicks ticks,
            IInterpolationTicks interpolationTicks)
        {
            _character = character;
            _guard = guard;
            _ticks = ticks;
            _interpolationTicks = interpolationTicks;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                _trigger.Begin(Input.GetMouseButton(0)); //TODO: new input system
                _pressTick = _ticks.Current - byte.MaxValue;
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => LatchPress());
                controlledLifetime.OnTermination(_trigger.Clear);
            });
            //TODO: оружие, поднятое на E с зажатой ЛКМ, сервер кладёт в руки раньше, чем сэмплер это увидит, и удержание сразу стреляет из него
            character.Hands.State.Select(lifetime, state => state.InHands).Advise(lifetime, _ => _trigger.Begin(Input.GetMouseButton(0))); //TODO: new input system
            guard.Held.Advise(lifetime, _ => _trigger.Begin(Input.GetMouseButton(0))); //TODO: new input system
        }

        public TriggerControls Sample(DrawControl choice)
        {
            var held = Input.GetMouseButton(0); //TODO: new input system
            if (choice.Kind != DrawControlKind.Keep)
                _trigger.CarryOver(held);

            var pressed = _trigger.Latched;
            var pulled = !_guard.Held.Value && _trigger.Pulled(held && HoldCounted);
            var viewTick = pressed ? _triggerViewTick : _interpolationTicks.InterpolationTick;
            var predictedTick = pressed ? _triggerPredictedTick : _ticks.PredictedViewTick(_interpolationTicks);
            var viewDelay = pulled ? (float)(_ticks.Current - viewTick) : 0f;
            var predictedDelay = pulled ? (float)(_ticks.Current - predictedTick) : 0f;
            if (pressed)
                RememberPress(viewDelay, predictedDelay);

            _trigger.Clear();
            return new TriggerControls(pulled, viewDelay, predictedDelay, LastPress);
        }

        private void LatchPress()
        {
            if (_guard.Held.Value || !Input.GetMouseButtonDown(0) || _trigger.Latched) //TODO: new input system
                return;

            _trigger.Latch();
            _triggerViewTick = _interpolationTicks.InterpolationTick;
            _triggerPredictedTick = _ticks.PredictedViewTick(_interpolationTicks);
        }

        private void RememberPress(float viewDelay, float predictedDelay)
        {
            _pressNumber++;
            _pressTick = _ticks.Current;
            _pressViewDelay = viewDelay;
            _pressPredictedDelay = predictedDelay;
        }
    }
}
