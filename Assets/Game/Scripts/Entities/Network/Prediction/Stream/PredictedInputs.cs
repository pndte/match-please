using Bw.Entities.Network.Rollback;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictedInputs<TInput> : IStepInputs<TInput> where TInput : struct
    {
        private readonly IPredictedBy _predictedBy;
        private readonly IReadonlyControlledBy _controlledBy;
        private readonly IReadonlyTickHistory<TInput> _own;
        private readonly IRemoteInputs<TInput> _remote;

        public PredictedInputs(
            IPredictedBy predictedBy,
            IReadonlyControlledBy controlledBy,
            IReadonlyTickHistory<TInput> own,
            IRemoteInputs<TInput> remote)
        {
            _predictedBy = predictedBy;
            _controlledBy = controlledBy;
            _own = own;
            _remote = remote;
        }

        public bool TryTakeInputAt(int tick, out TInput input)
        {
            if (!_predictedBy.Me.Value)
            {
                input = default;
                return false;
            }

            return _controlledBy.Me.Value ? _own.TryGetAt(tick, out input) : _remote.TryTakeInputAt(tick, out input);
        }
    }
}
