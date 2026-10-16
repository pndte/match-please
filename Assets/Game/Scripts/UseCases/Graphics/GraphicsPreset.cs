using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Bw.UseCases.Graphics
{
    [CreateAssetMenu(fileName = "GraphicsPreset", menuName = "Configs/GraphicsPreset")]
    public sealed class GraphicsPreset : ScriptableObject
    {
        public string Title;
        public UniversalRenderPipelineAsset Pipeline;
        public VolumeProfile PostOverrides;
        public bool LiveLampLight;
        public bool LiveLampHaze;
    }
}
