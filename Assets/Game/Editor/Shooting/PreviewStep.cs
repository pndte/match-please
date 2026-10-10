using System;
using Bw.Entities.Simulation;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Shooting
{
    public sealed class PreviewStep : ISimulationStep
    {
        private const string NetworkPrefab = "Assets/Game/Prefabs/Network/Network.prefab";

        public float Duration { get; }

        private PreviewStep(float duration)
        {
            Duration = duration;
        }

        public static PreviewStep FromNetwork()
        {
            var network = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkPrefab);
            if (network == null || !network.TryGetComponent<NetworkManager>(out var manager))
                throw new InvalidOperationException($"No NetworkManager at '{NetworkPrefab}': the weapon preview simulates at its tick rate.");

            return new PreviewStep(1f / manager.NetworkConfig.TickRate);
        }
    }
}
