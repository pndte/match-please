using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Props.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network.Prediction
{
    public sealed class ThrowPrediction //TODO: следующее оружие после броска достаёт сервер — до его ответа руки бросившего пусты
    {
        private readonly IReadonlySimulation<PropState> _simulation;
        private readonly PredictedHold _hold;
        private readonly IPredictedRigidBody _body;
        private readonly ReleaseSpots _spots;
        private readonly PropThrowConfig _config;

        private int _pending = int.MaxValue;

        public ThrowPrediction(
            Lifetime lifetime,
            IReadonlySimulation<PropState> simulation,
            PredictedHold hold,
            IPredictedRigidBody body,
            ReleaseSpots spots,
            PropThrowConfig config)
        {
            _simulation = simulation;
            _hold = hold;
            _body = body;
            _spots = spots;
            _config = config;

            simulation.Stepped.Advise(lifetime, Throw);
            simulation.State.Advise(lifetime, Verify);
        }

        private void Throw(PropState state) //TODO: точку броска берём от нарисованных рук, а вид отстаёт от симуляции на тик: у бегущего бросившего ящик стартует на тик бега позади серверного (0,43 на полном беге) и сдвигается к нему со сглаживанием, когда приходит первый снимок
        {
            if (state.Throws <= _simulation.Previous.Throws || !_hold.HeldLifetime.Value.IsAlive)
                return;

            var launched = _config.Thrown(_spots.NearestToHolder(), state.Aim);
            _pending = state.Throws;
            _hold.Release();
            _body.Launch(launched);
        }

        private void Verify(PropState state)
        {
            if (!_hold.IsReleasedEarly || state.Throws >= _pending)
                return;

            _pending = int.MaxValue;
            _hold.Restore();
        }
    }
}
