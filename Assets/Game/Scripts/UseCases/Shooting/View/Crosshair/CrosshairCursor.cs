using UnityEngine;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class CrosshairCursor : MonoBehaviour
    {
        [SerializeField] private Texture2D _texture;

        private void OnEnable() =>
            Cursor.SetCursor(_texture, new Vector2(_texture.width / 2f, _texture.height / 2f), CursorMode.Auto);

        private void OnDisable() =>
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
