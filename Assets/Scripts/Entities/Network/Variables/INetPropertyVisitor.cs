namespace Bw.Entities.Network.Variables
{
    public interface INetPropertyVisitor
    {
        public void Visit<T>(INetProperty<T> property);
    }
}