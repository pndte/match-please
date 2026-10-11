using UnityEngine;
using UnityEngine.UI;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarWidget : MonoBehaviour
    {
        public RectTransform Root => _root;
        public RectTransform Body => _body;
        public CanvasGroup Group => _group;
        public Graphic Bar => _bar;

        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _body;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Graphic _bar;
    }
}
