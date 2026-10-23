using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class HandsStateView : IStateView<HandsState>
    {
        private readonly ISimulation<HandsState> _hands;

        public HandsStateView(ISimulation<HandsState> hands)
        {
            _hands = hands;
        }

        public void Show(HandsState from, HandsState to, float progress) =>
            _hands.Apply(to);
    }
}
