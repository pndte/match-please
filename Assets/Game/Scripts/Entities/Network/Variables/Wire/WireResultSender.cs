namespace Bw.Entities.Network.Variables
{
    public sealed class WireResultSender<TValue, TWire> : INetResultSender<TValue>
    {
        private readonly INetResultSender<TWire> _sender;
        private readonly IWireForm<TValue, TWire> _form;

        public WireResultSender(INetResultSender<TWire> sender, IWireForm<TValue, TWire> form)
        {
            _sender = sender;
            _form = form;
        }

        public void Broadcast(TValue result) =>
            _sender.Broadcast(_form.WireOf(result));

        public void SendTo(IClient recipient, TValue result) =>
            _sender.SendTo(recipient, _form.WireOf(result));
    }
}
