using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Extensions
{
    public static class WeaponInventoryConfigExtensions
    {
        public static WeaponKindRules RulesOf(this WeaponInventoryConfig config, WeaponKind kind)
        {
            foreach (var rules in config.Rules)
                if (rules.Kind == kind)
                    return rules;

            throw new InvalidOperationException($"The inventory has no rules for {kind.ToString()} weapons: every kind needs a row of rules.");
        }
    }
}
