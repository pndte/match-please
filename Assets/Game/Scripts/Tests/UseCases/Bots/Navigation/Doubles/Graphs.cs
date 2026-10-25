using System.Collections.Generic;
using Bw.UseCases.Bots.Navigation;
using UnityEngine;

namespace Bw.Tests.UseCases.Bots.Navigation.Doubles
{
    public sealed class Graphs : System.IDisposable
    {
        private readonly List<NavGraphAsset> _assets = new();

        public NavGraphAsset Asset(Vector2[] nodes, params NavEdge[] edges)
        {
            var asset = ScriptableObject.CreateInstance<NavGraphAsset>();
            asset.name = "Test graph";
            asset.Nodes = nodes;
            asset.Edges = edges;
            _assets.Add(asset);
            return asset;
        }

        public NavGraph Graph(Vector2[] nodes, params NavEdge[] edges) =>
            NavGraph.Of(Asset(nodes, edges));

        public static NavEdge Edge(int from, int to, int ticks) =>
            new() { From = from, To = to, Ticks = ticks };

        public static Vector2[] Row(int count, float spacing = 1f)
        {
            var nodes = new Vector2[count];
            for (var index = 0; index < count; index++)
                nodes[index] = new Vector2(index * spacing, 0f);

            return nodes;
        }

        public void Dispose()
        {
            foreach (var asset in _assets)
                Object.DestroyImmediate(asset);

            _assets.Clear();
        }
    }
}
