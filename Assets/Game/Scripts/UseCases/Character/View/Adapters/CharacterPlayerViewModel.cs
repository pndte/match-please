using Bw.Entities;
using Bw.UseCases.Character.View.Player;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Character.View.Adapters
{
    public sealed class CharacterPlayerViewModel : IPlayerViewModel
    {
        public IReadonlyProperty<bool> Mine => _ownership.Mine;
        public IReadonlyProperty<bool> Controlled => _controlledBy.Me;
        public IReadonlyProperty<string> Nickname => _nickname.Value;

        private readonly IOwnership _ownership;
        private readonly IReadonlyControlledBy _controlledBy;
        private readonly IReadonlyNickname _nickname;

        public CharacterPlayerViewModel(IOwnership ownership, IReadonlyControlledBy controlledBy, IReadonlyNickname nickname)
        {
            _ownership = ownership;
            _controlledBy = controlledBy;
            _nickname = nickname;
        }
    }
}
