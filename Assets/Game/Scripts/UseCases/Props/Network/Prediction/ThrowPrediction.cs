using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Movement.Network.Prediction;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network.Prediction
{
    public sealed class ThrowPrediction
    {
        private readonly IWeapon _prop;
        private readonly IWeaponsInHands _inHands;
        private readonly ICarriedReplicas _replicas;
        private readonly IPredictedRigidBody _body;
        private readonly ThrowLaunch _launch;
        private readonly ThrowConfirmation _confirmation;

        public ThrowPrediction( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IWeapon prop,
            IWeaponsInHands inHands,
            ICarriedReplicas replicas,
            IPredictedRigidBody body,
            ThrowLaunch launch,
            ThrowConfirmation confirmation)
        {
            _prop = prop;
            _inHands = inHands;
            _replicas = replicas;
            _body = body;
            _launch = launch;
            _confirmation = confirmation;

            prop.Fired.Advise(lifetime, shot => Throw(shot.Aim));
        }

        private void Throw(float aim) //TODO: руки для точки броска берём нарисованными, а вид отстаёт от симуляции на тик: у бегущего бросившего ящик стартует на тик бега позади серверного (0,43 на полном беге) и сдвигается к нему со сглаживанием, когда приходит первый снимок
        {
            var holder = _inHands.HolderOf(_prop);
            var launched = _launch.Thrown(holder, aim);
            var replica = _replicas.ByCharacter[holder];
            _confirmation.Expect(replica);
            replica.DropAhead(_prop);
            _body.Launch(launched);
        }
    }
}
