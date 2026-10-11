using Bw.Entities;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Abstractions
{
    public interface IHealthBars
    {
        public void Show(Lifetime lifetime, IReadonlyHealth health, IReadonlyControlledBy controlledBy, Transform body);
    }
}
