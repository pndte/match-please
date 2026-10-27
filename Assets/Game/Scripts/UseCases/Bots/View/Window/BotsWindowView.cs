using Bw.UseCases.Bots.View.Roster;
using Bw.UseCases.Match.View.Hud;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bw.UseCases.Bots.View.Window
{
    public sealed class BotsWindowView
    {
        private const string Hidden = "hidden";
        private const KeyCode ToggleKey = KeyCode.B;

        private readonly IBotsWindowViewModel _window;
        private readonly ViewableProperty<bool> _open = new(false);

        public BotsWindowView(
            Lifetime lifetime,
            HudDocument hud,
            IBotsWindowViewModel window,
            IBotRosterViewModel roster)
        {
            _window = window;

            hud.WhenShown(lifetime, (shownLifetime, root) =>
            {
                var layer = root.Q("bots-layer");
                new BotRosterView(shownLifetime, layer, roster);
                window.Available.Advise(shownLifetime, available => _open.Value &= available);
                _open.Advise(shownLifetime, open => layer.EnableInClassList(Hidden, !open));
                Observable.EveryUpdate(UnityFrameProvider.Update, shownLifetime).Subscribe(_ => Toggle());
                shownLifetime.OnTermination(() => _open.Value = false);
            });
        }

        private void Toggle()
        {
            if (Input.GetKeyDown(ToggleKey) && _window.Available.Value) //TODO: new input system
                _open.Value = !_open.Value;
        }
    }
}
