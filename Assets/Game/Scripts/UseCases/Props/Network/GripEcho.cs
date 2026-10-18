using Bw.Entities.Extensions;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.Network
{
    public sealed class GripEcho
    {
        private readonly Transform _prop;
        private readonly BodyBox _shape;
        private readonly HeldPropGrip _echo;
        private readonly IInterpolatedLifetimes _lifetimes;
        private readonly SequentialLifetimes _echoes;

        public GripEcho(
            Lifetime lifetime,
            IWeaponHold hold,
            BodyBox shape,
            HeldPropGrip echo,
            IInterpolatedLifetimes lifetimes)
        {
            _prop = shape.transform;
            _shape = shape;
            _echo = echo;
            _lifetimes = lifetimes;
            _echoes = new SequentialLifetimes(lifetime);

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                _echoes.TerminateCurrent();
                heldLifetime.OnTermination(Echo);
            });
        }

        private void Echo()
        {
            var turn = _prop.rotation;
            _echo.Enter(_lifetimes.CreateUntilShown(_echoes.Next()));
            _echo.Place((Vector2)_prop.position + (Vector2)(turn * _shape.WorldOffset()), turn.eulerAngles.z);
        }
    }
}
