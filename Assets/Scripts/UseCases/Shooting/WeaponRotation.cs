using Bw.Entities.Network;
using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Shooting
{
    public class WeaponRotation : NetworkLifetimedBehaviour
    {
        [SerializeField] private WeaponRotationConfig _config;

        private Camera _camera;
        private Transform _parentTransform;

        private Vector3 _lastMouseWorldPosition;
        private bool _hasMouseWorldPosition;

        private float _currentAngle;
        private float _currentRadius;

        private void Awake()
        {
            _camera = Camera.main;
            _parentTransform = transform.parent;

            if (_config != null)
            {
                _currentRadius = _config.MaxOrbitRadius;
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                UpdateClientInput();
            }

            if (IsServer)
            {
                UpdateWeaponPosition();
            }
        }

        private void UpdateClientInput()
        {
            if (_camera == null) return;

            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = _camera.nearClipPlane;

            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);
            SendMouseWorldPositionServerRpc(mouseWorldPos);
        }

        [ServerRpc]
        private void SendMouseWorldPositionServerRpc(Vector3 mouseWorldPos)
        {
            _lastMouseWorldPosition = mouseWorldPos;
            _hasMouseWorldPosition = true;
        }

        private void UpdateWeaponPosition()
        {
            if (_config == null || _parentTransform == null || !_hasMouseWorldPosition) return;

            Vector3 delta = _lastMouseWorldPosition - _parentTransform.position;
            Vector2 directionFromParent = new Vector2(delta.x, delta.y);
            if (directionFromParent.sqrMagnitude < 0.0001f) return;
            directionFromParent.Normalize();

            float targetAngle = Mathf.Atan2(directionFromParent.y, directionFromParent.x) * Mathf.Rad2Deg;

            if (_config.RotationSpeed > 0f)
            {
                _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngle, _config.RotationSpeed * Time.deltaTime);
            }
            else
            {
                _currentAngle = targetAngle;
            }

            float angleInRadians = _currentAngle * Mathf.Deg2Rad;
            Vector2 directionToWeapon = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            var hit = Physics2D.Raycast(
                _parentTransform.position,
                directionToWeapon,
                _config.ObstacleDetectionDistance,
                _config.ObstacleLayerMask
            );

            bool obstacleDetected = hit.collider != null;

            float targetRadius = obstacleDetected ? _config.MinOrbitRadius : _config.MaxOrbitRadius;
            _currentRadius = Mathf.Lerp(_currentRadius, targetRadius, _config.RadiusChangeSpeed * Time.deltaTime);

            transform.localPosition = new Vector3(
                directionToWeapon.x * _currentRadius,
                directionToWeapon.y * _currentRadius,
                0f
            );

            if (_config.RotateWeaponTowardsMouse)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, _currentAngle);
            }

            Debug.DrawRay(_parentTransform.position, directionToWeapon * _config.ObstacleDetectionDistance,
                obstacleDetected ? Color.red : Color.green);
        }
    }
}
