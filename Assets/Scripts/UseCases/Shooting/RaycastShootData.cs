using Bw.Entities.Network;
using Bw.UseCases.Character;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Shooting
{
    public class RaycastShootData : NetworkLifetimedBehaviour
    {
        [SerializeField] private RaycastShootConfig _config;
        [SerializeField] private Transform _weaponMuzzleTransform;
        [SerializeField] private LineRenderer _trailPrefab;

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (!Input.GetMouseButtonDown(0)) return;

            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = _camera.nearClipPlane;

            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);
            SendMouseWorldPositionServerRpc(mouseWorldPos);
        }

        [ServerRpc]
        private void SendMouseWorldPositionServerRpc(Vector3 mouseWorldPosition)
        {
            Vector3 origin = _weaponMuzzleTransform != null ? _weaponMuzzleTransform.position : transform.position;
            Vector2 direction = ((Vector2)(mouseWorldPosition - origin)).normalized;
            if (direction.sqrMagnitude < 0.0001f) return;

            var hit = Physics2D.Raycast(origin, direction, _config.MaxDistance, _config.HitMask);

            Vector3 endPoint = hit.collider != null
                ? (Vector3)hit.point
                : origin + (Vector3)(direction * _config.MaxDistance);

            if (hit.collider != null && hit.collider.TryGetComponent<ICharacter>(out var character))
            {
                character.Health.Value -= _config.Damage;
            }

            SpawnTrailClientRpc(endPoint);
        }

        [ClientRpc]
        private void SpawnTrailClientRpc(Vector3 to)
        {
            PlayVfx(_weaponMuzzleTransform.position, to).Forget();
        }

        private async UniTaskVoid PlayVfx(Vector3 from, Vector3 to)
        {
            var trail = Instantiate(_trailPrefab, from, Quaternion.identity);
            trail.enabled = true;

            var distance = Vector3.Distance(from, to);
            var duration = distance / _config.TrailSpeed;

            float t = 0;
            await DOTween.To(() => t, x => t = x, 1f, duration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    trail.SetPosition(1, Vector3.Lerp(from, to, t));
                    trail.SetPosition(0, Vector3.Lerp(from, to, Mathf.Max(0, t - _config.TrailLength)));
                })
                .ToUniTask();

            Destroy(trail.gameObject);
        }
    }
}