namespace Bw.Entities.Network
{
    public interface IMessageSender // TODO:maybe not public
    {
    }

    public interface IMessageSender<T> : IMessageSender, IClientMessageSender<T>, IServerMessageSender<T>
    {
    }
}
