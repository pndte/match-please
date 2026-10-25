using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class BubbleHistory : IBubbleHistory
    {
        private const int KeptSwitches = 8;

        private readonly Dictionary<IClient, List<(int Tick, bool Inside)>> _switches = new();
        private readonly Transform _transform;
        private readonly IClientBubbles _bubbles;

        public BubbleHistory(
            Lifetime lifetime,
            BodyBox box,
            IClientBubbles bubbles,
            IClientCollection clients,
            IWorldStep worldStep)
        {
            _transform = box.transform;
            _bubbles = bubbles;

            clients.ByIds.View(lifetime, (clientLifetime, _, client) => Watch(clientLifetime, client));
            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, Settle);
        }

        public bool Inside(IClient client, double tick)
        {
            if (!_switches.TryGetValue(client, out var switches) || switches.Count == 0)
                return false;

            for (var index = switches.Count - 1; index >= 0; index--)
                if (switches[index].Tick <= tick)
                    return switches[index].Inside;

            return !switches[0].Inside;
        }

        private void Watch(Lifetime lifetime, IClient client)
        {
            _switches.Add(client, new List<(int Tick, bool Inside)>(KeptSwitches + 1));
            lifetime.OnTermination(() => _switches.Remove(client));
        }

        private void Settle(int tick)
        {
            Vector2 position = _transform.position;
            foreach (var (client, switches) in _switches)
            {
                var inside = switches.Count > 0 && switches[^1].Inside;
                if (_bubbles.Contains(client, position, inside) == inside)
                    continue;

                switches.Add((tick, !inside));
                if (switches.Count > KeptSwitches)
                    switches.RemoveAt(0);
            }
        }
    }
}
