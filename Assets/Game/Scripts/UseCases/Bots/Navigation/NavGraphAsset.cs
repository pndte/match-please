using System;
using UnityEngine;

namespace Bw.UseCases.Bots.Navigation
{
    public sealed class NavGraphAsset : ScriptableObject
    {
        public string Fingerprint = string.Empty;
        public Vector2[] Nodes = Array.Empty<Vector2>();
        public NavEdge[] Edges = Array.Empty<NavEdge>();
    }
}
