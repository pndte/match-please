using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Senses;
using UnityEngine;

namespace Bw.UseCases.Bots.Goap
{
    public sealed class TargetAssessor
    {
        private const float CloseSlack = 1.5f;
        private const float TargetHalfWidth = 0.4f;

        private readonly BotWeapons _weapons;
        private readonly BotPersonality _personality;
        private readonly BotsConfig _bots;

        public TargetAssessor(BotWeapons weapons, BotPersonality personality, BotsConfig bots)
        {
            _weapons = weapons;
            _personality = personality;
            _bots = bots;
        }

        public BotFact FactsAbout(Sighting target, Vector2 eye, BotFact previous)
        {
            if (!target.Visible)
                return BotFact.TargetNoticed;

            var facts = BotFact.TargetNoticed | BotFact.TargetInSight;
            var distance = (target.Position - eye).magnitude;
            if (distance <= _personality.CloseRange + ((previous & BotFact.TargetClose) != 0 ? CloseSlack : 0f))
                facts |= BotFact.TargetClose;

            if (distance <= _bots.Fists.Reach + _bots.Fists.Radius + TargetHalfWidth)
                facts |= BotFact.TargetInReach;

            if (_weapons.IsReloading(target.Character))
                facts |= BotFact.TargetReloading;

            if (target.Character.Mortal.Health.Share() <= _personality.WoundedHealth)
                facts |= BotFact.TargetWounded;

            return facts;
        }
    }
}
