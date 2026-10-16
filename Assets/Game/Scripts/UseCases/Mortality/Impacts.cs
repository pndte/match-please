using System;

namespace Bw.UseCases.Mortality
{
    public readonly struct Impacts
    {
        public int Count => All.Length;
        public Impact this[int index] => All[index];

        private ReadOnlySpan<Impact> All => _impacts;

        private readonly Impact[] _impacts;

        public Impacts(Impact impact)
        {
            _impacts = new[] { impact };
        }

        public Impacts(Impact[] impacts)
        {
            _impacts = impacts;
        }

        public ReadOnlySpan<Impact>.Enumerator GetEnumerator() =>
            All.GetEnumerator();
    }
}
