using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponInputSampler : IInputSampler<WeaponInput>
    {
        private const float MinAimDistance = 0.01f;

        private TriggerPress LastPress =>
            new(_pressNumber, (byte)Mathf.Min(_ticks.Current - _pressTick, byte.MaxValue), _pressViewDelay, _pressPredictedDelay, 0);

        private readonly Transform _weapon;
        private readonly IChangableCamera _camera;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolationTicks;
        private readonly TriggerLatch _trigger = new();

        private bool _drawn;
        private float _aim;
        private double _triggerViewTick;
        private double _triggerPredictedTick;
        private bool _reloadPressed;
        private byte _pressNumber;
        private int _pressTick;
        private float _pressViewDelay;
        private float _pressPredictedDelay;

        public WeaponInputSampler(
            Lifetime lifetime,
            IControlledBy controlledBy,
            IReadonlyWeaponDraw draw,
            IChangableCamera camera,
            INetworkTicks ticks,
            IInterpolationTicks interpolationTicks,
            Transform weapon)
        {
            _weapon = weapon;
            _camera = camera;
            _ticks = ticks;
            _interpolationTicks = interpolationTicks;

            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                _drawn = true;
                drawnLifetime.OnTermination(() => _drawn = false);
            });
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                _trigger.Begin(Input.GetMouseButton(0)); //TODO: new input system
                _pressTick = _ticks.Current - byte.MaxValue;
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => LatchPresses());
                controlledLifetime.OnTermination(ForgetPresses);
            });
        }

        public WeaponInput Sample()
        {
            var pressed = _trigger.Latched;
            var trigger = _trigger.Pulled(Input.GetMouseButton(0)); //TODO: new input system
            var viewTick = pressed ? _triggerViewTick : _interpolationTicks.InterpolationTick;
            var predictedTick = pressed ? _triggerPredictedTick : _ticks.PredictedViewTick(_interpolationTicks);
            var viewDelay = trigger ? (float)(_ticks.Current - viewTick) : 0f;
            var predictedDelay = trigger ? (float)(_ticks.Current - predictedTick) : 0f;
            if (pressed)
                RememberPress(viewDelay, predictedDelay);

            var input = new WeaponInput(SampleAim(), trigger, _reloadPressed, viewDelay, predictedDelay, LastPress);
            _trigger.Clear();
            _reloadPressed = false;
            return input;
        }

        private void LatchPresses()
        {
            if (Input.GetMouseButtonDown(0) && !_trigger.Latched) //TODO: new input system
            {
                _trigger.Latch();
                _triggerViewTick = _interpolationTicks.InterpolationTick;
                _triggerPredictedTick = _ticks.PredictedViewTick(_interpolationTicks);
            }

            if (Input.GetKeyDown(KeyCode.R)) //TODO: new input system
                _reloadPressed = true;
        }

        private void RememberPress(float viewDelay, float predictedDelay)
        {
            _pressNumber++;
            _pressTick = _ticks.Current;
            _pressViewDelay = viewDelay;
            _pressPredictedDelay = predictedDelay;
        }

        private void ForgetPresses()
        {
            _trigger.Clear();
            _reloadPressed = false;
        }

        private float SampleAim()
        {
            if (!_drawn)
                return _aim;

            var camera = _camera.Current.Value;
            var mouse = Input.mousePosition; //TODO: new input system
            mouse.z = camera.nearClipPlane;

            var target = (Vector2)_weapon.Holder().InverseTransformPoint(camera.ScreenToWorldPoint(mouse));
            if (target.sqrMagnitude > MinAimDistance * MinAimDistance)
                _aim = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;

            return _aim;
        }
    }
}
