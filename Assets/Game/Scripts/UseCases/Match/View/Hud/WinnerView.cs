using System;
using System.Globalization;
using System.Linq;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class WinnerView
    {
        private const string Hidden = "hidden";
        private const string FigurePrefix = "winner__figure--";

        private readonly IMatchViewModel _match;
        private int _shownSeconds = -1;

        public WinnerView(Lifetime lifetime, HudDocument hud, IMatchViewModel match)
        {
            _match = match;

            hud.WhenShown(lifetime, (shownLifetime, root) =>
            {
                var feed = root.Q("kill-feed-layer");
                var layer = root.Q("winner-layer");
                var next = layer.Q<Label>("next");
                match.Sheet.Advise(shownLifetime, sheet => Show(feed, layer, sheet));
                Observable.EveryUpdate(UnityFrameProvider.Update, shownLifetime).Subscribe(_ => Count(next));
            });
        }

        private void Show(VisualElement feed, VisualElement layer, ScoreSheet sheet)
        {
            var ended = sheet.Stage == MatchStage.Ended && sheet.Lines.Count > 0;
            feed.EnableInClassList(Hidden, ended);
            layer.EnableInClassList(Hidden, !ended);
            if (!ended)
                return;

            var winner = sheet.Lines[0];
            layer.Q<Label>("title").text = winner.Mine ? "Ты победил!" : "Победитель матча";
            var figure = layer.Q("figure");
            foreach (var shown in figure.GetClasses().Where(name => name.StartsWith(FigurePrefix, StringComparison.Ordinal)).ToArray())
                figure.RemoveFromClassList(shown);

            figure.AddToClassList(FigurePrefix + winner.Figure);
            layer.Q<Label>("nickname").text = winner.Nickname;
            ScoreLabels.Fill(layer, winner.Stats);
            _shownSeconds = -1;
        }

        private void Count(Label next)
        {
            if (_match.Sheet.Value.Stage != MatchStage.Ended)
                return;

            var seconds = _match.SecondsLeft;
            if (seconds == _shownSeconds)
                return;

            _shownSeconds = seconds;
            next.text = $"Новый матч через {seconds.ToString(CultureInfo.InvariantCulture)} с";
        }
    }
}
