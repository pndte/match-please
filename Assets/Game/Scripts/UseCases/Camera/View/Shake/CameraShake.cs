using UnityEngine;
using UnityEngine.Rendering;

namespace Bw.UseCases.Camera.View.Shake
{
    public sealed class CameraShake : MonoBehaviour, ICameraShake
    {
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private CameraShakeConfig _config;

        private float _strength;
        private Vector2 _kick;
        private Vector2 _pan;
        private Vector2 _lastPosition;
        private Vector3 _restPosition;
        private Quaternion _restRotation;

        public void Shake(float strength) =>
            _strength = Mathf.Clamp01(_strength + strength);

        public void Kick(Vector2 offset) =>
            _kick = Vector2.ClampMagnitude(_kick + AgainstPan(offset), _config.MaxKick);

        private void OnEnable()
        {
            _lastPosition = _camera.transform.position;
            RenderPipelineManager.beginCameraRendering += Apply;
            RenderPipelineManager.endCameraRendering += Restore;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= Apply;
            RenderPipelineManager.endCameraRendering -= Restore;
        }

        private void Update()
        {
            _strength = Mathf.Max(0f, _strength - _config.Decay * Time.deltaTime);
            _kick *= Mathf.Exp(-_config.KickReturn * Time.deltaTime);
        }

        private void LateUpdate()
        {
            var position = (Vector2)_camera.transform.position;
            if (Time.deltaTime > 0f)
                _pan = (position - _lastPosition) / Time.deltaTime;
            _lastPosition = position;
        }

        private Vector2 AgainstPan(Vector2 offset)
        {
            if (Vector2.Dot(offset.normalized, _pan) <= _config.MaskingPanSpeed)
                return offset;

            var panDirection = _pan.normalized;
            return offset - 2f * Vector2.Dot(offset, panDirection) * panDirection;
        }

        private void Apply(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            if (camera != _camera)
                return;

            var cameraTransform = _camera.transform;
            _restPosition = cameraTransform.localPosition;
            _restRotation = cameraTransform.localRotation;

            var shake = _strength * _strength;
            var time = Time.time * _config.Frequency;
            var offset = new Vector2(Noise(time, 0.3f), Noise(time, 7.1f)) * (_config.MaxOffset * shake) + _kick;
            var angle = Noise(time, 13.7f) * _config.MaxAngle * shake;

            cameraTransform.localPosition = _restPosition + (Vector3)offset;
            cameraTransform.localRotation = _restRotation * Quaternion.Euler(0f, 0f, angle);
        }

        private void Restore(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            if (camera != _camera)
                return;

            var cameraTransform = _camera.transform;
            cameraTransform.localPosition = _restPosition;
            cameraTransform.localRotation = _restRotation;
        }

        private static float Noise(float time, float row) =>
            Mathf.PerlinNoise(time, row) * 2f - 1f;
    }
}
