using System.Collections.Generic;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Doubles
{
    public sealed class RecordedShovable : IShovable
    {
        public float Retreat { get; set; }
        public List<Vector2> Shoves { get; } = new();

        public float SpeedAway(Vector2 direction) =>
            Retreat;

        public void Shove(Vector2 force) =>
            Shoves.Add(force);
    }
}
