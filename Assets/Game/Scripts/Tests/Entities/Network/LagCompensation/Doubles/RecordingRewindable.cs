using System.Collections.Generic;
using Bw.Entities.Network.LagCompensation;
using JetBrains.Lifetimes;

namespace Bw.Tests.Entities.Network.LagCompensation.Doubles
{
    public sealed class RecordingRewindable : IRewindable
    {
        public IReadOnlyList<(Lifetime Lifetime, ShooterView View)> Rewound => _rewound;

        private readonly List<(Lifetime, ShooterView)> _rewound = new();

        public void Rewind(Lifetime lifetime, ShooterView view) =>
            _rewound.Add((lifetime, view));
    }
}
