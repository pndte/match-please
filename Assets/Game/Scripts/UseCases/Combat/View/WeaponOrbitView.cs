using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Combat.View
{
    public sealed class WeaponOrbitView
    {
        private readonly Transform _weapon;
        private readonly IWeaponPoseViewModel _pose;

        private float _radius;

        public WeaponOrbitView(
            Lifetime lifetime,
            Transform weapon,
            IWeaponCarryViewModel carry,
            IWeaponPoseViewModel pose)
        {
            _weapon = weapon;
            _pose = pose;

            carry.Drawn.WhenTrue(lifetime, drawnLifetime =>
            {
                _radius = pose.Radius;
                Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate, drawnLifetime).Subscribe(_ => Place(Time.deltaTime));
            });
        }

        private void Place(float deltaTime)
        {
            var pivot = _pose.Pivot;
            var rotation = Quaternion.Euler(0f, 0f, _pose.Aim);
            _radius = Mathf.Lerp(_radius, _pose.Radius, _pose.RadiusSpeed * deltaTime);
            var orbit = (Vector2)(rotation * Vector3.right) * _radius;
            _weapon.SetPositionAndRotation(new Vector3(pivot.x + orbit.x, pivot.y + orbit.y, _weapon.position.z), rotation);
        }
    }
}
