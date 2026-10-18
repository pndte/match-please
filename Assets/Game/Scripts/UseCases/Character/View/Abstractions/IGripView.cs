using System.Collections.Generic;
using Bw.UseCases.Character.View.Grips;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Abstractions
{
    public interface IGripView
    {
        public void Grip(Lifetime lifetime, IReadOnlyList<HandGrip> grips, SpriteRenderer held);
    }
}
