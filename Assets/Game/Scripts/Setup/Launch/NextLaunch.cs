using System;

namespace Setup.Launch
{
    public sealed class NextLaunch
    {
        private Start _start = new Initial();

        public void OpenMenu() =>
            _start = new Menu();

        public void StartWith(LaunchChoice choice) =>
            _start = new Chosen(choice);

        public TResult Switch<TResult>(Func<TResult> initial, Func<TResult> menu, Func<LaunchChoice, TResult> chosen) =>
            _start.Switch(initial, menu, chosen);

        private abstract class Start
        {
            public abstract TResult Switch<TResult>(Func<TResult> initial, Func<TResult> menu, Func<LaunchChoice, TResult> chosen);
        }

        private sealed class Initial : Start
        {
            public override TResult Switch<TResult>(Func<TResult> initial, Func<TResult> menu, Func<LaunchChoice, TResult> chosen) =>
                initial();
        }

        private sealed class Menu : Start
        {
            public override TResult Switch<TResult>(Func<TResult> initial, Func<TResult> menu, Func<LaunchChoice, TResult> chosen) =>
                menu();
        }

        private sealed class Chosen : Start
        {
            private readonly LaunchChoice _choice;

            public Chosen(LaunchChoice choice)
            {
                _choice = choice;
            }

            public override TResult Switch<TResult>(Func<TResult> initial, Func<TResult> menu, Func<LaunchChoice, TResult> chosen) =>
                chosen(_choice);
        }
    }
}
