using Entities;
using Entities.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class NetworkMovementData : NetworkLifetimedBehaviour
    {
        public NetworkVariable<float> XVelocity = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        private Rigidbody2D _physics;
        private MovementConfig _config;

        [Inject]
        public void Construct(Rigidbody2D physics, MovementConfig config)
        {
            _physics = physics;
            _config = config;
        }
        
        private void Update() //TODO: свой Loop;
        {
            if (IsOwner)
            {
                XVelocity.Value = Input.GetAxisRaw("Horizontal");
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    JumpServerRpc();
                }
            }
            else if (IsServer) // TODO: сделать отдельный процессор (cs) для этого
            {
                _physics.linearVelocity = _physics.linearVelocity.WithX(XVelocity.Value * (_config.Speed * Time.deltaTime));
            }
        }

        [ServerRpc]
        private void JumpServerRpc()
        {
            _physics.AddForce(Vector2.up * _config.JumpForce);
        }
    }
}