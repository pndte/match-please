using System;

namespace Bw.Entities.Network.Rollback
{
    public interface IReadonlyTickHistory<T>
    {
        public int NewestTick { get; }

        public bool TryGetAt(int tick, out T value);
        public bool TryGetLatest(int notAfter, out int tick, out T value);
        public bool TryGetEarliest(int notBefore, out int tick, out T value);
    }

    public interface ITickHistory<T> : IReadonlyTickHistory<T>
    {
        public void Record(int tick, T value);
        public void Clear();
    }

    public class TickHistory<T> : ITickHistory<T>
    {
        public int NewestTick { get; private set; } = int.MinValue;
        public int Capacity => _ticks.Length;

        private readonly T[] _values;
        private readonly int[] _ticks;

        public TickHistory(int capacity)
        {
            _values = new T[capacity];
            _ticks = new int[capacity];
            Array.Fill(_ticks, int.MinValue);
        }

        public void Record(int tick, T value)
        {
            if (tick == int.MinValue)
                throw new ArgumentOutOfRangeException(nameof(tick), "int.MinValue marks an empty slot and is never a recorded tick.");

            if (NewestTick != int.MinValue && tick <= NewestTick - _ticks.Length)
                throw new ArgumentOutOfRangeException(nameof(tick), $"The history keeps the newest {_ticks.Length.ToString()} ticks up to {NewestTick.ToString()}, and tick {tick.ToString()} is older: a receiver drops such late ticks itself.");

            var slot = SlotOf(tick);
            _ticks[slot] = tick;
            _values[slot] = value;
            if (tick > NewestTick)
                NewestTick = tick;
        }

        public void Clear()
        {
            Array.Fill(_ticks, int.MinValue);
            Array.Clear(_values, 0, _values.Length);
            NewestTick = int.MinValue;
        }

        public bool TryGetAt(int tick, out T value)
        {
            var slot = SlotOf(tick);
            if (tick != int.MinValue && _ticks[slot] == tick && tick > NewestTick - _ticks.Length)
            {
                value = _values[slot];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetLatest(int notAfter, out int tick, out T value)
        {
            if (NewestTick != int.MinValue)
                for (tick = Math.Min(notAfter, NewestTick); tick > NewestTick - _ticks.Length; tick--)
                    if (TryGetAt(tick, out value))
                        return true;

            tick = default;
            value = default;
            return false;
        }

        public bool TryGetEarliest(int notBefore, out int tick, out T value)
        {
            if (NewestTick != int.MinValue)
                for (tick = Math.Max(notBefore, NewestTick - _ticks.Length + 1); tick <= NewestTick; tick++)
                    if (TryGetAt(tick, out value))
                        return true;

            tick = default;
            value = default;
            return false;
        }

        private int SlotOf(int tick) =>
            ((tick % _ticks.Length) + _ticks.Length) % _ticks.Length;
    }
}
