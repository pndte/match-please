using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Bw.EditorTools.Content
{
    public sealed class ContentKeyBuildChecker : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var assets = ContentKeys.KeyedAssets();
            var unkeyed = assets.Where(asset => asset.Key == 0).Select(AssetDatabase.GetAssetPath).ToArray();
            if (unkeyed.Length > 0)
                throw new BuildFailedException(
                    $"Content without a key: {string.Join(", ", unkeyed)}. The editor issues keys after every import and compilation; open the project in the main editor first.");

            var repeated = assets
                .GroupBy(asset => asset.Key)
                .Where(group => group.Count() > 1)
                .Select(group => $"{group.Key.ToString()} in {string.Join(", ", group.Select(AssetDatabase.GetAssetPath))}")
                .ToArray();
            if (repeated.Length > 0)
                throw new BuildFailedException(
                    $"Content keys repeat: {string.Join("; ", repeated)}. The editor gives a copy a new key after the next import or compilation; open the project in the main editor first.");
        }
    }
}
