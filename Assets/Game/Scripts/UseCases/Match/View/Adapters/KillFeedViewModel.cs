using Bw.UseCases.Match.View.Hud;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.View.Adapters
{
    public sealed class KillFeedViewModel : IKillFeedViewModel
    {
        public ISource<KillLine> Killed => _killed;

        private readonly Signal<KillLine> _killed = new();
        private readonly WeaponIconsConfig _icons;

        public KillFeedViewModel(Lifetime lifetime, IReadonlyKillFeed feed, WeaponIconsConfig icons)
        {
            _icons = icons;

            feed.Reported.Advise(lifetime, kill => _killed.Fire(LineOf(kill)));
        }

        private KillLine LineOf(KillNote kill) =>
            new(NameOf(kill.Killer), NameOf(kill.Victim), _icons.IconOf(kill.Weapon));

        private static KillName NameOf(KillParty party) =>
            new(party.Nickname, party.IsMine);
    }
}
