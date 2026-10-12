using Bw.Entities.Players;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Character
{
    public interface IReadonlyNickname
    {
        public IReadonlyProperty<string> Value { get; }
    }

    public interface INickname : IReadonlyNickname
    {
        public void Apply(string nickname);
    }

    public sealed class Nickname : INickname
    {
        public IReadonlyProperty<string> Value => _value;

        private readonly ViewableProperty<string> _value = new(NicknameRules.Fallback);

        public void Apply(string nickname) =>
            _value.Value = nickname;
    }
}
