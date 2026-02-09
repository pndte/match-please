using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeapon
    {
        /// <summary>
        /// Shot event, vector3 argument is a mouse position in global space
        /// </summary>
        public ISource<Vector3> Shot { get; }
        public IReadonlyProperty<bool> ReadyToShot { get; }
    }
}