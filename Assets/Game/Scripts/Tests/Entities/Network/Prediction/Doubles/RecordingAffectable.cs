using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class RecordingAffectable<TEffect> : IAffectable<TEffect> where TEffect : struct
    {
        public IReadOnlyList<(StateCause Cause, TEffect Effect)> Affected => _affected;

        private readonly List<(StateCause, TEffect)> _affected = new();
        private readonly List<string> _order;
        private readonly string _name;

        public RecordingAffectable(List<string> order = null, string name = "")
        {
            _order = order;
            _name = name;
        }

        public void Affect(StateCause cause, TEffect effect)
        {
            _affected.Add((cause, effect));
            _order?.Add($"{_name} affected");
        }
    }
}
