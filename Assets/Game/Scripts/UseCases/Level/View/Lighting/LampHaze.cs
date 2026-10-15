using Bw.UseCases.Level.View.Abstractions;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Bw.UseCases.Level.View.Lighting
{
    public sealed class LampHaze : MonoBehaviour, ILampHaze
    {
        [SerializeField] private Light2D[] _lamps;
        [SerializeField] private GameObject _baked;

        public void ShowLive() =>
            Show(true);

        public void ShowBaked() =>
            Show(false); //TODO: запечённая дымка не знает о движущемся: персонажи, ящики и кабины лифтов не отбрасывают в ней тени, в живой (пресет «Максимум») отбрасывают

        private void Show(bool live)
        {
            foreach (var lamp in _lamps)
            {
                lamp.volumetricEnabled = live;
                lamp.volumetricShadowsEnabled = live;
            }

            _baked.SetActive(!live);
        }
    }
}
