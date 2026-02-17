using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Movement.Network
{
    public class NetworkMovementData : NetworkLifetimedBehaviour
    {
        [HideInInspector]
        public NetworkVariable<float> XInput = new(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private Rigidbody2D _physics;
        private MovementConfig _config;
        private bool _isGrounded;

        private readonly List<Collider2D> _groundResults = new(2);
        private ContactFilter2D _groundFilter;

        [Inject]
        public void Construct(Rigidbody2D physics, MovementConfig config)
        {
            _physics = physics;
            _config = config;

            _groundFilter.useLayerMask = true;
            _groundFilter.layerMask = _config.GroundLayer; 
            _groundFilter.useTriggers = false; 
        }

        private void Update()
        {
            if (!IsOwner) return;

            XInput.Value = Input.GetAxisRaw("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                JumpServerRpc();
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;

            const float checkRadius = 0.2f; 
            var checkPosition = (Vector2)transform.position + Vector2.down;

            var hitCount = Physics2D.OverlapCircle(checkPosition, checkRadius, _groundFilter, _groundResults);
            _isGrounded = hitCount > 0;

            _physics.linearVelocity = _physics.linearVelocity.WithX(Mathf.Clamp(XInput.Value, -1, 1) * _config.Speed);
        }

        [ServerRpc]
        private void JumpServerRpc()
        {
            if (!_isGrounded) return;
            
            _physics.AddForce(Vector2.up * _config.JumpForce, ForceMode2D.Impulse);
            _isGrounded = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            var checkPosition = (Vector2)transform.position + Vector2.down * 0.1f;
            Gizmos.DrawWireSphere(checkPosition, 0.2f);
        }
    }
}
