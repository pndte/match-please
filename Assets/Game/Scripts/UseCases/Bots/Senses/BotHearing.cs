using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Bots.Senses
{
    public sealed class BotHearing //TODO: бот слышит выстрел по линии, а не пулю: выстрел мимо, но в его сторону, он тоже принимает за атаку
    {
        public int Heard { get; private set; }
        public IReadonlyCharacter Shooter { get; private set; }

        private Vector2 Position => _self.Movement.State.Value.Body.Position;

        private readonly IReadonlyCharacter _self;
        private readonly BotsConfig _bots;

        public BotHearing(
            Lifetime lifetime,
            IReadonlyCharacter self,
            IReadonlyWeaponsInHands inHands,
            BotsConfig bots)
        {
            _self = self;
            _bots = bots;

            inHands.ByWeapon.View(lifetime, (heldLifetime, weapon, holder) =>
            {
                if (holder != self)
                    weapon.Fired.Advise(heldLifetime, shot => Hear(holder, shot));
            });
        }

        private void Hear(IReadonlyCharacter shooter, WeaponShot shot)
        {
            if (!IsAimedAtMe(shooter.Hands.Pose.Position, shot.Aim))
                return;

            Shooter = shooter;
            Heard++;
        }

        private bool IsAimedAtMe(Vector2 from, float aim)
        {
            var direction = (Vector2)(Quaternion.Euler(0f, 0f, aim) * Vector3.right);
            var toMe = Position - from;
            var along = Vector2.Dot(toMe, direction);
            return along > 0f && along <= _bots.ThreatRange && Mathf.Abs(direction.x * toMe.y - direction.y * toMe.x) <= _bots.ThreatRadius;
        }
    }
}
