using Bw.Entities;
using Bw.UseCases.Bots.View.Window;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Bots.View.Adapters
{
    public sealed class BotsWindowViewModel : IBotsWindowViewModel
    {
        public IReadonlyProperty<bool> Available => _ownership.Mine;

        private readonly IOwnership _ownership;

        public BotsWindowViewModel(IOwnership ownership)
        {
            _ownership = ownership;
        }
    }
}
