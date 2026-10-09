using System;

namespace Bw.Entities.Network.Prediction.Events.Requests
{
    public readonly struct StateCause
    {
        public static readonly StateCause None = default;

        internal bool IsAction { get; }
        internal ActionId Action { get; }

        private StateCause(ActionId action)
        {
            IsAction = true;
            Action = action;
        }

        public static StateCause Of(ActionId action) =>
            new(action);

        public void Switch<TState>(TState state, Action<TState> none, Action<TState, ActionId> action)
        {
            if (IsAction)
                action(state, Action);
            else
                none(state);
        }
    }
}
