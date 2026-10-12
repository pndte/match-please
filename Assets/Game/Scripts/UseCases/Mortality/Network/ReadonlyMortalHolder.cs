using Bw.Entities;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Mortality.Network
{
    public sealed class ReadonlyMortalHolder : MonoBehaviour, IHolder<IReadonlyMortal>
    {
        public IReadonlyMortal Value { get; private set; }

        [Inject]
        private void Construct(IReadonlyMortal mortal)
        {
            Value = mortal;
        }
    }
}
