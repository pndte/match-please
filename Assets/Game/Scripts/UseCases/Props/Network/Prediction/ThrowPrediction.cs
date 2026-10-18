using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Network.Prediction;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network.Prediction
{
    public sealed class ThrowPrediction //TODO: следующее оружие после броска достаёт сервер — до его ответа руки бросившего пусты
    {
        private readonly IReadonlySimulation<PropState> _simulation;
        private readonly PredictedHold _hold;
        private readonly IPredictedRigidBody _body;
        private readonly PropStateView _pose;
        private readonly ThrowLaunch _launch;

        private int _pending = int.MaxValue;

        public ThrowPrediction(
            Lifetime lifetime,
            IReadonlySimulation<PropState> simulation,
            PredictedHold hold,
            IPredictedRigidBody body,
            PropStateView pose,
            ThrowLaunch launch)
        {
            _simulation = simulation;
            _hold = hold;
            _body = body;
            _pose = pose;
            _launch = launch;

            simulation.Stepped.Advise(lifetime, Throw);
            simulation.State.Advise(lifetime, Verify);
        }

        private void Throw(PropState state) //TODO: держателя для точки броска берём нарисованным, а вид отстаёт от симуляции на тик: у бегущего бросившего ящик стартует на тик бега позади серверного (0,43 на полном беге) и сдвигается к нему со сглаживанием, когда приходит первый снимок
        {
            if (state.Throws <= _simulation.Previous.Throws || !_hold.HeldLifetime.Value.IsAlive)
                return;

            _pose.Show(state, state, 1f);
            var launched = _launch.FromHands(state.Aim);
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
