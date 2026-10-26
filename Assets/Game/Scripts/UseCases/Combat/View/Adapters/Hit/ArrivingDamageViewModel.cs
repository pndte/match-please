using System;
using Bw.Entities.Extensions;
using Bw.Entities.Physics;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.View.Hit;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Adapters.Hit
{
    public sealed class ArrivingDamageViewModel : IDamageViewModel
    {
        public ISource<Unit> Taken => _taken;

        private Vector2 Center => _box.transform.TransformPoint(_box.Offset);

        private readonly Signal<Unit> _taken = new();
        private readonly Lifetime _lifetime;
        private readonly IReadonlyMortal _mortal;
        private readonly BodyBox _box;

        private double _landsAt = double.MinValue;

        public ArrivingDamageViewModel(
            Lifetime lifetime,
            IDamageViewModel damage,
            IReadonlyMortal mortal,
            ShotLaunches launches,
            BodyBox box)
        {
            _lifetime = lifetime;
            _mortal = mortal;
            _box = box;

            launches.Launched.Advise(lifetime, Remember);
            damage.Taken.Advise(lifetime, _ => TakeOnArrival());
        }

        private void Remember(ShotLaunch launch)
        {
            if (launch.Target == _mortal)
                _landsAt = Time.timeAsDouble + Vector2.Distance(launch.From, Center) / launch.Speed;
        }

        private void TakeOnArrival() =>
            _lifetime.WhenElapsed((float)Math.Max(0d, _landsAt - Time.timeAsDouble), () => _taken.Fire(Unit.Instance));
    }
}
