using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Bw.UseCases.Level.View.Lighting
{
    [ExecuteAlways]
    [RequireComponent(typeof(Light2D))]
    public sealed class AmbientLight : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _level = 1f;
        [SerializeField] private Color _darkTint = new(0.65f, 0.76f, 1f);

        private void OnEnable() =>
            Apply();

        private void OnValidate() =>
            Apply();

        private void Apply()
        {
            var light = GetComponent<Light2D>();
            light.intensity = Mathf.GammaToLinearSpace(_level);
            light.color = Color.Lerp(_darkTint, Color.white, _level).linear;
        }
    }
}
