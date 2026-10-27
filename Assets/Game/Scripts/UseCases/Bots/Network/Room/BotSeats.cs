using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class BotSeats : IBotSeats
    {
        public IReadonlyProperty<IReadOnlyList<BotSeat>> Seats => _seats;

        private readonly ViewableProperty<IReadOnlyList<BotSeat>> _seats = new(Array.Empty<BotSeat>());

        public void Apply(IReadOnlyList<BotSeat> seats) =>
            _seats.Value = seats;
    }
}
