namespace Bw.Entities.Network.LagCompensation
{
    public interface IBubbleHistory
    {
        public bool Inside(IClient client, double tick);
    }
}
