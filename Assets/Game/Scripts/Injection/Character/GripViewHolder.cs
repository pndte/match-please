using Bw.Entities;
using Bw.UseCases.Character.View.Abstractions;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class GripViewHolder : MonoBehaviour, IHolder<IGripView>
    {
        public IGripView Value { get; private set; }

        [Inject]
        private void Construct(IGripView view)
        {
            Value = view;
        }
    }
}
