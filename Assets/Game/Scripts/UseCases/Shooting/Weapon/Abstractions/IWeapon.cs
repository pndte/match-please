using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeapon
    {
        /// <summary>
        /// Shot event, vector3 argument is a shot target position in global space
        /// </summary>
        public ISource<Vector3> OnShot { get; }
        public void Shoot(Vector3 targetPosition);
        public IReadonlyProperty<bool> CanShoot { get; }
    }
}