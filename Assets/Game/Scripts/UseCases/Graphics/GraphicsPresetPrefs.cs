using UnityEngine;

namespace Bw.UseCases.Graphics
{
    public sealed class GraphicsPresetPrefs
    {
        private const string PresetPref = "Graphics.Preset";

        public GraphicsPreset Saved
        {
            get
            {
                var saved = PlayerPrefs.GetString(PresetPref, _presets.Default.name);
                foreach (var preset in _presets.Presets)
                    if (preset.name == saved)
                        return preset;

                return _presets.Default;
            }
        }

        private readonly GraphicsPresetsConfig _presets;

        public GraphicsPresetPrefs(GraphicsPresetsConfig presets) =>
            _presets = presets;

        public void Save(GraphicsPreset preset)
        {
            PlayerPrefs.SetString(PresetPref, preset.name);
            PlayerPrefs.Save();
        }
    }
}
