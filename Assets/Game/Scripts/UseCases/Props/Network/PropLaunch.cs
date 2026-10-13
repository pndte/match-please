using System;
using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Props.Network
{
    public abstract class PropLaunch
    {
        public static readonly PropLaunch Dropped = new DroppedLaunch();

        private PropLaunch()
        {
        }

        public static PropLaunch Thrown(IReadonlyMortal thrower, Vector2 velocity) =>
            new ThrownLaunch(thrower, velocity);

        public abstract void Switch(Action dropped, Action<IReadonlyMortal, Vector2> thrown);

        private sealed class DroppedLaunch : PropLaunch
        {
            public override void Switch(Action dropped, Action<IReadonlyMortal, Vector2> thrown) =>
                dropped();
        }

        private sealed class ThrownLaunch : PropLaunch
        {
            private readonly IReadonlyMortal _thrower;
            private readonly Vector2 _velocity;

            public ThrownLaunch(IReadonlyMortal thrower, Vector2 velocity)
            {
                _thrower = thrower;
                _velocity = velocity;
            }

            public override void Switch(Action dropped, Action<IReadonlyMortal, Vector2> thrown) =>
                thrown(_thrower, _velocity);
        }
    }
}
