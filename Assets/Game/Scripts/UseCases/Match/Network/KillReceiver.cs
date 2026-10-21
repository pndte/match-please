using Bw.CoreAdapters.Weapons;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class KillReceiver
    {
        private readonly ILocalPlayer _localPlayer;
        private readonly IKillFeed _feed;
        private readonly WeaponCatalog _weapons;

        public KillReceiver(
            Lifetime lifetime,
            ILocalPlayer localPlayer,
            INetResultReceiver<KillNotice> notices,
            IKillFeed feed,
            WeaponCatalog weapons)
        {
            _localPlayer = localPlayer;
            _feed = feed;
            _weapons = weapons;

            notices.Received.Advise(lifetime, Receive);
        }

        private void Receive(KillNotice notice) =>
            _feed.Report(new KillNote(PartyOf(notice.Killer), PartyOf(notice.Victim), _weapons.WeaponOf(notice.WeaponKey)));

        private KillParty PartyOf(PlayerTag tag) =>
            new(tag.Nickname, _localPlayer.Id.Value.Is(tag.Id));
    }
}
