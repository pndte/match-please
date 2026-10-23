using System;

namespace Bw.UseCases.Props.Network
{
    public abstract class PropLaunch
    {
        public static readonly PropLaunch Dropped = new DroppedLaunch();

        private PropLaunch()
        {
        }

        public static PropLaunch Thrown(float aim) => //TODO: пуллирование or smth, лишние аллокации на пустом месте
            new ThrownLaunch(aim);

        public abstract void Switch(Action dropped, Action<float> thrown);

        private sealed class DroppedLaunch : PropLaunch
        {
            public override void Switch(Action dropped, Action<float> thrown) =>
                dropped();
        }

        private sealed class ThrownLaunch : PropLaunch
        {
            private readonly float _aim;

            public ThrownLaunch(float aim)
            {
                _aim = aim;
            }

            public override void Switch(Action dropped, Action<float> thrown) =>
                thrown(_aim);
        }
    }
}
