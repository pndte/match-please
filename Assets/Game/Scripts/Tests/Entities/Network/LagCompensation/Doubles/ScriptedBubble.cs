using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;

namespace Bw.Tests.Entities.Network.LagCompensation.Doubles
{
    public sealed class ScriptedBubble : IBubbleHistory
    {
        public bool Holds { get; set; }
        public List<(IClient Client, double Tick)> Asked { get; } = new();

        public bool Inside(IClient client, double tick)
        {
            Asked.Add((client, tick));
            return Holds;
        }
    }
}
