using Bw.Entities.Simulation;
using Bw.UseCases.Props.Extensions;
using UnityEngine;

namespace Bw.UseCases.Props
{
    public sealed class PropSwing
    {
        private readonly HeldPropConfig _config;
        private readonly ISimulationStep _step;

        public PropSwing(HeldPropConfig config, ISimulationStep step)
        {
            _config = config;
            _step = step;
        }

        public float AngleOf(PropState state)
        {
            if (state.Phase != PropPhase.WindingUp)
                return 0f;

            var progress = Mathf.Clamp01(state.PhaseTicks / (float)_config.WindUpTicks(_step));
            var side = Mathf.Sign(Mathf.Cos(state.Aim * Mathf.Deg2Rad));
            return side * _config.WindUpAngle * _config.WindUpSwing.Evaluate(progress);
        }
    }
}
