using System;
using Bw.UseCases.Mortality;

namespace Bw.UseCases.Props.Network
{
    public abstract class PropLaunch
    {
        public static readonly PropLaunch Dropped = new DroppedLaunch();

        private PropLaunch()
        {
        }

        public static PropLaunch Thrown(IReadonlyMortal thrower, float aim) =>
            new ThrownLaunch(thrower, aim);

        public abstract void Switch(Action dropped, Action<IReadonlyMortal, float> thrown);

        private sealed class DroppedLaunch : PropLaunch
        {
            public override void Switch(Action dropped, Action<IReadonlyMortal, float> thrown) =>
                dropped();
        }

        private sealed class ThrownLaunch : PropLaunch
        {
            private readonly IReadonlyMortal _thrower;
            private readonly float _aim;

            public ThrownLaunch(IReadonlyMortal thrower, float aim)
            {
                _thrower = thrower;
                _aim = aim;
            }

            public override void Switch(Action dropped, Action<IReadonlyMortal, float> thrown) =>
                thrown(_thrower, _aim);
        }
    }
}
