using System.Collections.Generic;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Bots.Network.Room
{
    public interface IReadonlyBotSeats
    {
        public IReadonlyProperty<IReadOnlyList<BotSeat>> Seats { get; }
    }

    public interface IBotSeats : IReadonlyBotSeats
    {
        public void Apply(IReadOnlyList<BotSeat> seats);
    }
}
