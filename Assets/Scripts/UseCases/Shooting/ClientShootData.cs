using System;
using Bw.Entities.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting
{
    [Obsolete]
    public class ClientShootData : NetworkLifetimedBehaviour
    {
        [SerializeField] private NetworkBehaviour _bulletPrefab;
        [SerializeField] private Transform _weaponMuzzleTransform;
        
        private Camera _camera;
        private IInstantiator _instantiator;

        [Inject]
        private void Construct(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseScreenPos = Input.mousePosition;
                mouseScreenPos.z = _camera.nearClipPlane;
                
                Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);
                
                SendMousePositionServerRpc(mouseWorldPos);
            }
        }

        [ServerRpc]
        public void SendMousePositionServerRpc(Vector3 mouseWorldPosition)
        {
            var direction = ((Vector2)(mouseWorldPosition - transform.position)).normalized;

            Shoot(direction);
        }

        private void Shoot(Vector2 direction)
        {
            var prefab = _instantiator.InstantiatePrefabForComponent<Bullet>(_bulletPrefab,
                _weaponMuzzleTransform.position, Quaternion.identity, null);
            prefab.GetComponent<NetworkLifetimedBehaviour>().NetworkObject.Spawn(true);
            prefab.Launch(direction);
        }
    }
}