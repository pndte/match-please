using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Props.View.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Props.View.Adapters
{
    public sealed class HeldPropViewModel : IHeldPropViewModel
    {
        public Vector2 Position
        {
            get
            {
                var grip = _inHands.HolderOf(_prop).Hands.Grip;
                return grip.Center - (Vector2)(Quaternion.Euler(0f, 0f, grip.Angle) * _shape.WorldOffset());
            }
        }

        public float Angle => _inHands.HolderOf(_prop).Hands.Grip.Angle;

        private readonly IWeapon _prop;
        private readonly IWeaponsInHands _inHands;
        private readonly BodyBox _shape;

        public HeldPropViewModel(IWeapon prop, IWeaponsInHands inHands, BodyBox shape)
        {
            _prop = prop;
            _inHands = inHands;
            _shape = shape;
        }
    }
}
