using Bw.UseCases.Level.View.Abstractions;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Bw.UseCases.Level.View.Lighting
{
    public sealed class LampLight : MonoBehaviour, ILampLight
    {
        [SerializeField] private Light2D[] _lamps;
        [SerializeField] private GameObject _baked;

        public void ShowLive() =>
            Show(true);

        public void ShowBaked() =>
            Show(false); //TODO: в запечённом свете ламп персонажи, ящики и кабины лифтов не отбрасывают от них теней, а рельеф нормалей от ламп лежит множителем только на кусках карты и задевает и свет фонарика

        private void Show(bool live)
        {
            foreach (var lamp in _lamps)
                lamp.enabled = live;

            _baked.SetActive(!live);
        }
    }
}
