using Bw.Entities;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarNickname
    {
        public HealthBarNickname(
            Lifetime lifetime,
            HealthBarWidget widget,
            IReadonlyNickname nickname,
            IOwnership ownership)
        {
            nickname.Value.Advise(lifetime, value => widget.Nickname.text = value);
            ownership.Mine.Advise(lifetime, mine => widget.Nickname.enabled = !mine);
        }
    }
}
