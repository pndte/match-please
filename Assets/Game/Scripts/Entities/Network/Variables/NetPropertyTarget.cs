using System;

namespace Bw.Entities.Network.Variables
{
    internal abstract class NetPropertyTarget
    {
        public static readonly NetPropertyTarget Everyone = new EveryoneTarget();

        private NetPropertyTarget()
        {
        }

        public static NetPropertyTarget Except(IClient writer) =>
            new ExceptTarget(writer ?? throw new ArgumentNullException(nameof(writer)));

        public abstract void Switch<TState>(TState state, Action<TState> everyone, Action<TState, IClient> except);

        private sealed class EveryoneTarget : NetPropertyTarget
        {
            public override void Switch<TState>(TState state, Action<TState> everyone, Action<TState, IClient> except) =>
                everyone(state);
        }

        private sealed class ExceptTarget : NetPropertyTarget
        {
            private readonly IClient _writer;

            public ExceptTarget(IClient writer)
            {
                _writer = writer;
            }

            public override void Switch<TState>(TState state, Action<TState> everyone, Action<TState, IClient> except) =>
                except(state, _writer);
        }
    }
}
