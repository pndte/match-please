using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class PendingReloadLifetimes //tODO: интерфейс и универсализация, если понадобится для других объектов
    {
        private readonly Dictionary<ulong, LifetimeDefinition> _byRequestId = new();

        public void Register(ulong requestId, LifetimeDefinition definition) =>
            _byRequestId[requestId] = definition;

        public bool TryRelease(ulong requestId, bool cancel)
        {
            if (!_byRequestId.Remove(requestId, out var definition))
                return false;

            if (cancel)
                definition.Terminate();

            return true;
        }
    }
}
