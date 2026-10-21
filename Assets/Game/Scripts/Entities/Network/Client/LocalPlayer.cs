using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public sealed class LocalPlayer : ILocalPlayer
    {
        public IReadonlyProperty<LocalPlayerId> Id => _id;

        private readonly IViewableProperty<LocalPlayerId> _id = new ViewableProperty<LocalPlayerId>(LocalPlayerId.Unknown);

        public void Set(Lifetime lifetime, int id)
        {
            if (_id.Value.IsKnown)
                throw new InvalidOperationException("The local player is already known: Set names it for one lifetime, and the previous one has not ended yet.");

            _id.Value = LocalPlayerId.Of(id);
            lifetime.OnTermination(() => _id.Value = LocalPlayerId.Unknown);
        }
    }
}
