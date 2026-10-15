using UnityEngine;

namespace Bw.UseCases.Graphics
{
    [CreateAssetMenu(fileName = "GraphicsPresetsConfig", menuName = "Configs/GraphicsPresetsConfig")]
    public sealed class GraphicsPresetsConfig : ScriptableObject
    {
        public GraphicsPreset[] Presets;
        public GraphicsPreset Default;
    }
}
