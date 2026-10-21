using Bw.Entities.Network;
using Bw.UseCases.Match.Network.Extensions;
using Bw.UseCases.Match.Network.Requests;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Match.Network
{
    public sealed class KillNotices
    {
        private readonly IReadonlyPlayerIds _ids;
        private readonly IReadonlyWeaponDescriptionRegistry _descriptions;

        public KillNotices(IReadonlyPlayerIds ids, IReadonlyWeaponDescriptionRegistry descriptions)
        {
            _ids = ids;
            _descriptions = descriptions;
        }

        public KillNotice Of(Kill kill) =>
            new(_ids.TagOf(kill.Killer), _ids.TagOf(kill.Victim), _descriptions.DescriptionOf(kill.Weapon).Key);
    }
}
