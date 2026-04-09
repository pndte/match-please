namespace Bw.Entities.Network.Variables
{
    public interface INetSyncVisitor
    {
        void VisitProperty<T>(INetProperty<T> property);
        void VisitSignal<T>(INetSignal<T> signal);
    }
}
