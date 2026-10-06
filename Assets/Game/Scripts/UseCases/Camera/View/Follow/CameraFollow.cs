using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Camera.View.Follow
{
    public sealed class CameraFollow
    {
        private readonly ICameraTargets _targets;
        private readonly UnityEngine.Camera _camera;
        private readonly CameraFollowConfig _config;

        private Vector2 _anchor;
        private Vector2 _anchorVelocity;
        private Vector2 _lookAhead;
        private Vector2 _lookAheadVelocity;

        public CameraFollow(Lifetime lifetime, ICameraTargets targets, UnityEngine.Camera camera, CameraFollowConfig config)
        {
            _targets = targets;
            _camera = camera;
            _config = config;

            targets.View(lifetime, (_, target) => Snap(target));
            Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate, lifetime).Subscribe(Follow);
        }

        private void Snap(Transform target)
        {
            _anchor = (Vector2)target.position + _config.Offset;
            _anchorVelocity = Vector2.zero;
            _lookAhead = Vector2.zero;
            _lookAheadVelocity = Vector2.zero;
            Place();
        }

        private void Follow(Unit _)
        {
            if (_targets.Count == 0)
                return;

            var anchor = (Vector2)_targets[_targets.Count - 1].position + _config.Offset;
            _anchor.x = Mathf.SmoothDamp(_anchor.x, anchor.x, ref _anchorVelocity.x, _config.HorizontalSmoothing);
            _anchor.y = Mathf.SmoothDamp(_anchor.y, anchor.y, ref _anchorVelocity.y, _config.VerticalSmoothing);
            _lookAhead = Vector2.SmoothDamp(_lookAhead, LookAhead(), ref _lookAheadVelocity, _config.LookAheadSmoothing);
            Place();
        }

        private Vector2 LookAhead()
        {
            var cursor = (Vector2)_camera.ScreenToViewportPoint(Input.mousePosition) * 2f - Vector2.one; //TODO: new input system
            var reach = Mathf.Pow(Mathf.Min(cursor.magnitude, 1f), _config.LookAheadCurve);
            return Vector2.Scale(cursor.normalized * reach, _config.MaxLookAhead);
        }

        private void Place()
        {
            var cameraTransform = _camera.transform;
            var position = _anchor + _lookAhead;
            cameraTransform.position = new Vector3(position.x, position.y, cameraTransform.position.z);
        }
    }
}
