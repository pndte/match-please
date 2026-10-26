using System.Collections.Generic;
using Bw.CoreAdapters.Weapons;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class WeaponPrefabKinds
    {
        public IReadOnlyDictionary<GameObject, WeaponKind> ByPrefab { get; }

        private WeaponPrefabKinds(IReadOnlyDictionary<GameObject, WeaponKind> byPrefab)
        {
            ByPrefab = byPrefab;
        }

        public static WeaponPrefabKinds Of(WeaponCatalog catalog, IReadOnlyList<NetworkObject> props)
        {
            var byPrefab = new Dictionary<GameObject, WeaponKind>();
            foreach (var entry in catalog.Weapons)
                byPrefab.Add(entry.Prefab.gameObject, entry.Kind);

            foreach (var prop in props)
                byPrefab.Add(prop.gameObject, WeaponKind.Prop);

            return new WeaponPrefabKinds(byPrefab);
        }
    }
}
