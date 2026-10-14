using System;
using Bw.Entities;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Shooting.View.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Rig
{
    public sealed class BodyPunchPose
    {
        private readonly Transform _weapon;

        public BodyPunchPose(Lifetime lifetime, Transform weapon, IMeleeSwingsViewModel swings)
        {
            _weapon = weapon;

            swings.Started.Advise(lifetime, _ => Body().Punch());
        }

        private ICharacterPoses Body()
        {
            var holder = _weapon.parent;
            if (holder == null)
                throw new InvalidOperationException($"Melee weapon '{_weapon.name}' swings only in hands, yet nobody holds it.");

            if (!holder.TryGetComponent<IHolder<ICharacterPoses>>(out var poses))
                throw new InvalidOperationException($"'{holder.name}' holds melee weapon '{_weapon.name}' but has no poses to strike.");

            return poses.Value;
        }
    }
}
