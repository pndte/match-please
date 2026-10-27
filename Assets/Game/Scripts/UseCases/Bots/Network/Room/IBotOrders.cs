namespace Bw.UseCases.Bots.Network.Room
{
    public interface IBotOrders
    {
        public void Add(string nickname, BotPreset preset);
        public void Remove(BotSeat seat);
        public void Change(BotSeat seat, BotPreset preset);
    }
}
