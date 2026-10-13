using Bw.Entities;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class HandsHolder : MonoBehaviour, IHolder<IHands>
    {
        public IHands Value { get; private set; }

        [Inject]
        private void Construct(IHands hands)
        {
            Value = hands;
        }
    }
}
