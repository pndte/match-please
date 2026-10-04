namespace Bw.Entities.Network.Variables
{
    internal interface INetSyncVisitor
    {
        void VisitProperty<T>(INetProperty<T> property);
        void VisitSignal<T>(INetSignal<T> signal);
    }
}
