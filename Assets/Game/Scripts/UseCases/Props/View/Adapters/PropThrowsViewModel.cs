using Bw.Entities.Simulation;
using Bw.UseCases.Props.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using UnityEngine;

namespace Bw.UseCases.Props.View.Adapters
{
    public sealed class PropThrowsViewModel : IStateView<PropState>, IPropThrowsViewModel
    {
        public ISource<Unit> WindUpStarted => _windUpStarted;
        public ISource<PropThrow> Thrown => _thrown;

        private readonly Signal<Unit> _windUpStarted = new();
        private readonly Signal<PropThrow> _thrown = new();

        private bool _synchronized;
        private bool _windingUp;
        private int _throws;

        public void Show(PropState from, PropState to, float progress)
        {
            var windingUp = to.Phase == PropPhase.WindingUp;
            var started = _synchronized && windingUp && !_windingUp;
            var thrown = _synchronized && to.Throws > _throws;
            _windingUp = windingUp;
            _throws = to.Throws;
            _synchronized = true;

            if (started)
                _windUpStarted.Fire(Unit.Instance);

            if (thrown)
                _thrown.Fire(new PropThrow(new Vector2(Mathf.Cos(to.Aim * Mathf.Deg2Rad), Mathf.Sin(to.Aim * Mathf.Deg2Rad))));
        }
    }
}
