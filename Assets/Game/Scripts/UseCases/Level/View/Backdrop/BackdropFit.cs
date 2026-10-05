using UnityEngine;

namespace Bw.UseCases.Level.View.Backdrop
{
    [DefaultExecutionOrder(999)]
    public sealed class BackdropFit : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField, Min(0.01f)] private float _layoutOrthographicSize = 7.3f;

        private void LateUpdate()
        {
            var fit = _camera.orthographicSize / _layoutOrthographicSize;
            transform.localScale = new Vector3(fit, fit, 1f);
        }
    }
}
