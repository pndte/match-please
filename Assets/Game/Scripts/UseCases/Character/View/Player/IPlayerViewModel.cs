using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Character.View.Player
{
    public interface IPlayerViewModel
    {
        public IReadonlyProperty<bool> Mine { get; }
        public IReadonlyProperty<bool> Controlled { get; }
        public IReadonlyProperty<string> Nickname { get; }
    }
}
