using Bw.UseCases.Character.View.Player;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarNickname
    {
        public HealthBarNickname(Lifetime lifetime, HealthBarWidget widget, IPlayerViewModel player)
        {
            player.Nickname.Advise(lifetime, value => widget.Nickname.text = value);
            player.Mine.Advise(lifetime, mine => widget.Nickname.enabled = !mine);
        }
    }
}
