using Bw.Entities.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class ShovableRegistration
    {
        public ShovableRegistration(
            Lifetime lifetime,
            BoxCollider2D collider,
            IShovable shovable,
            IShovableCollection shovables)
        {
            shovables.AddLifetimed(lifetime, collider, shovable);
        }
    }
}
