using System.Collections.Generic;
using Bw.UseCases.Shooting.View.Abstractions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Bw.UseCases.Shooting.View.Muzzle
{
    public sealed class FlashlightBeam : MonoBehaviour, IFlashlightBeam
    {
        [SerializeField] private Light2D _cone;
        [SerializeField] private Light2D _spill;

        public void Show() =>
            Shine(true);

        public void Hide() =>
            Shine(false);

        private void OnEnable() =>
            RenderPipelineManager.beginContextRendering += AlignToBarrel;

        private void OnDisable() =>
            RenderPipelineManager.beginContextRendering -= AlignToBarrel;

        private void AlignToBarrel(ScriptableRenderContext context, List<UnityEngine.Camera> cameras) =>
            transform.rotation = Quaternion.LookRotation(Vector3.forward, transform.parent.localToWorldMatrix.MultiplyVector(Vector3.right));

        private void Shine(bool shining)
        {
            _cone.enabled = shining;
            _spill.enabled = shining;
        }
    }
}
