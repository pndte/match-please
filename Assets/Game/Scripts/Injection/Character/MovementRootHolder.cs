using Bw.Entities;
using Bw.UseCases.Movement.Network.Root;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class MovementRootHolder : MonoBehaviour, IHolder<IMovementRoot>
    {
        public IMovementRoot Value { get; private set; }

        [Inject]
        private void Construct(IMovementRoot root)
        {
            Value = root;
        }
    }
}
