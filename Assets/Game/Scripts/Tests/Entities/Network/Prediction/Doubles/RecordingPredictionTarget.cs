using System.Collections.Generic;
using System.Globalization;
using Bw.Entities.Network.Prediction.Events;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class RecordingPredictionTarget : IPredictionTarget<float>
    {
        public IReadOnlyList<string> Calls => _calls;

        private readonly List<string> _calls = new();

        public void Show(PredictedEffect<float> effect) =>
            _calls.Add($"show {Of(effect.Action)} {effect.Value.ToString(CultureInfo.InvariantCulture)}");

        public void Confirm(ActionId action, float actual) =>
            _calls.Add($"confirm {Of(action)} {actual.ToString(CultureInfo.InvariantCulture)}");

        public void Reject(ActionId action) =>
            _calls.Add($"reject {Of(action)}");

        private static string Of(ActionId action) =>
            $"{action.Initiator.ToString()}@{action.Tick.ToString()}";
    }
}
