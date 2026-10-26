namespace Bw.Entities.Network.Variables
{
    public interface INetEntries
    {
        public INetRequestSender<T> RequestSender<T>(NetRequestDeclaration<T> declaration);
        public INetRequestReceiver<T> RequestReceiver<T>(NetRequestDeclaration<T> declaration);
        public INetResultSender<T> ResultSender<T>(NetResultDeclaration<T> declaration);
        public INetResultReceiver<T> ResultReceiver<T>(NetResultDeclaration<T> declaration);
        public INetProperty<T> Property<T>(NetPropertyDeclaration<T> declaration);
    }
}
