using Bw.Entities;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class MovementHolder : MonoBehaviour, IHolder<IReadonlyMovement>
    {
        public IReadonlyMovement Value { get; private set; }

        [Inject]
        private void Construct(IReadonlyMovement movement)
        {
            Value = movement;
        }
    }
}
