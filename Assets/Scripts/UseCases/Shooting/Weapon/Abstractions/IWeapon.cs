using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeapon : IReadonlyWeapon
    {
        public void ForceShot(Vector3 targetPosition);
    }
    
    public interface IReadonlyWeapon
    {
        /// <summary>
        /// Shot event, vector3 argument is a shot target position in global space
        /// </summary>
        public ISource<Vector3> Shot { get; }
        public IReadonlyProperty<bool> ReadyToShot { get; }
    }
}