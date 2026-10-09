using System;

namespace Bw.Entities.Network.Prediction.Events
{
    public readonly struct ChangeCause<TState, TEffect> where TState : struct where TEffect : struct
    {
        public static readonly ChangeCause<TState, TEffect> Correction = default;

        private readonly Kind _kind;
        private readonly TEffect _effect;
        private readonly TState _state;

        private ChangeCause(Kind kind, TEffect effect, TState state)
        {
            _kind = kind;
            _effect = effect;
            _state = state;
        }

        public static ChangeCause<TState, TEffect> Predicted(TEffect effect) =>
            new(Kind.Predicted, effect, default);

        public static ChangeCause<TState, TEffect> Authoritative(TState state) =>
            new(Kind.Authoritative, default, state);

        public TResult Switch<TContext, TResult>(
            TContext context,
            Func<TContext, TEffect, TResult> predicted,
            Func<TContext, TState, TResult> authoritative,
            Func<TContext, TResult> correction)
        {
            if (_kind == Kind.Predicted)
                return predicted(context, _effect);

            if (_kind == Kind.Authoritative)
                return authoritative(context, _state);

            return correction(context);
        }

        private enum Kind : byte
        {
            //Может прийти и с сервака, и с клиента. По сути это коррекция без change
            Correction,
            //Своё локальное состояние
            Predicted,
            //Состояние, пришедшее с сервера
            Authoritative
        }
    }
}
