using System.Globalization;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class ScoreboardView
    {
        private const string Hidden = "hidden";
        private const KeyCode HoldKey = KeyCode.Tab;

        private readonly IReadonlyMatchBoard _board;
        private readonly VisualTreeAsset _rowTemplate;
        private MatchStandings _drawn = MatchStandings.None;

        public ScoreboardView(
            Lifetime lifetime,
            HudDocument hud,
            VisualTreeAsset rowTemplate,
            IReadonlyMatchBoard board)
        {
            _board = board;
            _rowTemplate = rowTemplate;

            hud.WhenShown(lifetime, (shownLifetime, root) =>
            {
                var layer = root.Q("scoreboard-layer");
                var rows = layer.Q("rows");
                Observable.EveryUpdate(UnityFrameProvider.Update, shownLifetime).Subscribe(_ => Refresh(layer, rows));
            });
        }

        private void Refresh(VisualElement layer, VisualElement rows)
        {
            var standings = _board.Current.Value;
            var shown = Input.GetKey(HoldKey) && standings.Period.Phase != MatchPhase.Ended; //TODO: new input system
            layer.EnableInClassList(Hidden, !shown);
            if (!shown || ReferenceEquals(standings, _drawn))
                return;

            _drawn = standings;
            Draw(rows, standings);
        }

        private void Draw(VisualElement rows, MatchStandings standings)
        {
            rows.Clear();
            for (var index = 0; index < standings.Lines.Count; index++)
            {
                var line = standings.Lines[index];
                var row = _rowTemplate.Instantiate().Q("row");
                row.EnableInClassList("score-row--leader", index == 0 && line.Score.Kills > 0);
                row.EnableInClassList("score-row--me", line.IsMine);
                row.EnableInClassList("score-row--odd", index % 2 == 1 && !line.IsMine);
                row.Q<Label>("place").text = (index + 1).ToString(CultureInfo.InvariantCulture);
                row.Q<Label>("nickname").text = line.Nickname;
                ScoreLabels.Fill(row, line.Score);
                rows.Add(row);
            }
        }
    }
}
