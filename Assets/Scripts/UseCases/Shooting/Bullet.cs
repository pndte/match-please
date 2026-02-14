using System;
using Bw.Entities.Network;
using Bw.UseCases.Character;
using UnityEngine;

namespace Bw.UseCases.Shooting
{
    [Obsolete]
    public class Bullet : NetworkLifetimedBehaviour
    {
        public float Speed;
        public Rigidbody2D Physics;

        private bool _launched;
        private Vector3 _direction;

        public void Launch(Vector3 direction)
        {
            _direction = direction;
            _launched = true;
        }

        private void Update()
        {
            if (_launched)
            {
                Physics.linearVelocity = _direction * Speed;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!IsServer) return;
            if (other.gameObject.layer != LayerMask.NameToLayer("Character")) NetworkObject.Despawn(true);
            if (!other.gameObject.TryGetComponent<ICharacter>(out var character)) return;
            character.Health.Value -= 10;
            NetworkObject.Despawn(true);
        }
    }
}