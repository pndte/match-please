using System.Collections.Generic;
using Bw.Entities.Network.Rollback;
using JetBrains.Lifetimes;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class RecordingRollbackParts : IRollbackParts
    {
        public IReadOnlyList<(Lifetime Lifetime, IRollbackPart Part)> Tracked => _tracked;
        public IReadOnlyList<(Lifetime Lifetime, IDivergenceSource Source)> Watched => _watched;

        private readonly List<(Lifetime, IRollbackPart)> _tracked = new();
        private readonly List<(Lifetime, IDivergenceSource)> _watched = new();

        public void Track(Lifetime lifetime, IRollbackPart part) =>
            _tracked.Add((lifetime, part));

        public void Watch(Lifetime lifetime, IDivergenceSource source) =>
            _watched.Add((lifetime, source));
    }
}
