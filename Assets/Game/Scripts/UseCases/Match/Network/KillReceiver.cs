using Bw.Entities.Network.Variables;
using Bw.UseCases.Match.Network.Requests;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.UseCases.Match.Network
{
    public sealed class KillReceiver
    {
        private readonly NetworkObject _networkObject;
        private readonly IKillFeed _feed;
        private readonly WeaponCatalog _weapons;

        public KillReceiver(
            Lifetime lifetime,
            NetworkObject networkObject,
            INetResultReceiver<KillNotice> notices,
            IKillFeed feed,
            WeaponCatalog weapons)
        {
            _networkObject = networkObject;
            _feed = feed;
            _weapons = weapons;

            notices.Received.Advise(lifetime, Receive);
        }

        private void Receive(KillNotice notice) =>
            _feed.Report(new KillNote(PartyOf(notice.Killer), PartyOf(notice.Victim), _weapons.WeaponOf(notice.WeaponKey)));

        private KillParty PartyOf(PlayerTag tag) =>
            new(tag.Nickname, tag.ClientId == _networkObject.NetworkManager.LocalClientId);
    }
}
