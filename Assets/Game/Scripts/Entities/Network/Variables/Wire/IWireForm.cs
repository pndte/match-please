namespace Bw.Entities.Network.Variables
{
    public interface IWireForm<TValue, TWire>
    {
        public TWire WireOf(TValue value);
        public TValue ValueOf(TWire wire);
    }
}
