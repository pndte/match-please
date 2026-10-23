using System.Collections.Generic;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public readonly struct SeenShot
    {
        public readonly Vector2 Direction;
        public readonly IReadOnlyList<PelletTrace> Pellets;

        public SeenShot(Vector2 direction, IReadOnlyList<PelletTrace> pellets)
        {
            Direction = direction;
            Pellets = pellets;
        }
    }
}
