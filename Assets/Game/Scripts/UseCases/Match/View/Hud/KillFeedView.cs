using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class KillFeedView
    {
        private const string Entering = "kill--entering";
        private const string Leaving = "kill--leaving";
        private const string MyKill = "kill--mine";
        private const string MyDeath = "kill--mine-death";
        private const string MyName = "kill__name--me";

        private readonly KillFeedConfig _config;

        public KillFeedView(
            Lifetime lifetime,
            HudDocument hud,
            IKillFeedViewModel feed,
            KillFeedConfig config)
        {
            _config = config;

            hud.WhenShown(lifetime, (shownLifetime, root) =>
            {
                var tiles = root.Q("kill-feed");
                feed.Killed.Advise(shownLifetime, kill => Show(tiles, kill));
            });
        }

        private void Show(VisualElement tiles, KillLine kill)
        {
            var tile = _config.Tile.Instantiate().Q("kill");
            Fill(tile, kill);
            tile.AddToClassList(Entering);
            tile.RegisterCallbackOnce<GeometryChangedEvent>(_ => tile.RemoveFromClassList(Entering));
            tiles.Insert(0, tile);
            tile.schedule.Execute(() => Leave(tile)).ExecuteLater(Milliseconds(_config.ShownSeconds));
            LeaveOverLimit(tiles);
        }

        private void LeaveOverLimit(VisualElement tiles)
        {
            var staying = 0;
            for (var index = 0; index < tiles.childCount; index++)
            {
                var tile = tiles[index];
                if (tile.ClassListContains(Leaving))
                    continue;

                staying++;
                if (staying > _config.Limit)
                    Leave(tile);
            }
        }

        private void Leave(VisualElement tile)
        {
            if (tile.ClassListContains(Leaving))
                return;

            tile.AddToClassList(Leaving);
            tile.schedule.Execute(tile.RemoveFromHierarchy).ExecuteLater(Milliseconds(_config.FadeSeconds));
        }

        private static void Fill(VisualElement tile, KillLine kill)
        {
            tile.EnableInClassList(MyKill, kill.Killer.Mine);
            tile.EnableInClassList(MyDeath, kill.Victim.Mine);
            Write(tile.Q<Label>("killer"), kill.Killer);
            Write(tile.Q<Label>("victim"), kill.Victim);
            ShowIcon(tile.Q("weapon"), kill.WeaponIcon);
        }

        private static void Write(Label label, KillName name)
        {
            label.text = name.Nickname;
            label.EnableInClassList(MyName, name.Mine);
        }

        private static void ShowIcon(VisualElement weapon, Sprite icon)
        {
            weapon.style.backgroundImage = new StyleBackground(icon);
            weapon.style.aspectRatio = new StyleRatio(icon.rect.width / icon.rect.height);
        }

        private static long Milliseconds(float seconds) =>
            (long)(seconds * 1000f);
    }
}
