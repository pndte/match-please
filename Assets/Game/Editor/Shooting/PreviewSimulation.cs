using System;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.EditorTools.Shooting
{
    public interface IPreviewSimulation
    {
        public float Aim { get; }
        public int Shots { get; }
        public int Accepted { get; }

        public void Step(WeaponInput input);
    }

    public sealed class PreviewSimulation<TState> : IPreviewSimulation where TState : struct, IWeaponState
    {
        public float Aim => _state.Aim;
        public int Shots => _state.Shots;
        public int Accepted { get; private set; }

        private readonly IWeaponSimulator<TState> _simulator;
        private readonly Func<TState, TState, bool> _accepts;

        private TState _state;

        public PreviewSimulation(IWeaponSimulator<TState> simulator, TState initial, Func<TState, TState, bool> accepts)
        {
            _simulator = simulator;
            _accepts = accepts;
            _state = initial;
        }

        public void Step(WeaponInput input)
        {
            var next = _simulator.Step(_state, input);
            if (_accepts(_state, next))
                Accepted++;

            _state = next;
        }
    }
}
