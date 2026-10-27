using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Bots.Network.Requests;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class BotOrderSender : IBotOrders
    {
        private readonly INetRequestSender<RoomBot> _joins;
        private readonly INetRequestSender<int> _removals;
        private readonly INetRequestSender<BotPresetChange> _changes;

        public BotOrderSender(
            INetRequestSender<RoomBot> joins,
            INetRequestSender<int> removals,
            INetRequestSender<BotPresetChange> changes)
        {
            _joins = joins;
            _removals = removals;
            _changes = changes;
        }

        public void Add(string nickname, BotPreset preset) =>
            _joins.Send(new RoomBot(nickname, preset.Difficulty.Key, preset.Personality.Key));

        public void Remove(BotSeat seat) =>
            _removals.Send(seat.PlayerId);

        public void Change(BotSeat seat, BotPreset preset) =>
            _changes.Send(new BotPresetChange(seat.PlayerId, preset.Difficulty.Key, preset.Personality.Key));
    }
}
