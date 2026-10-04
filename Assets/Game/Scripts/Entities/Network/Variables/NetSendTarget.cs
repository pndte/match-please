using System;

namespace Bw.Entities.Network.Variables
{
    // it's smth like union
    internal abstract class NetSendTarget
    {
        public static readonly NetSendTarget Untargeted = new UntargetedSend();

        public abstract bool IsTargeted { get; }

        private NetSendTarget()
        {
        }

        public static NetSendTarget To(IClient recipient) =>
            new TargetedSend(recipient ?? throw new ArgumentNullException(nameof(recipient)));

        public abstract void Switch(Action untargeted, Action<IClient> targeted);

        private sealed class UntargetedSend : NetSendTarget
        {
            public override bool IsTargeted => false;

            public override void Switch(Action untargeted, Action<IClient> targeted) =>
                untargeted();
        }

        private sealed class TargetedSend : NetSendTarget
        {
            public override bool IsTargeted => true;

            private readonly IClient _recipient;

            public TargetedSend(IClient recipient)
            {
                _recipient = recipient;
            }

            public override void Switch(Action untargeted, Action<IClient> targeted) =>
                targeted(_recipient);
        }
    }
}
