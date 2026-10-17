using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    public sealed class RigPose
    {
        public Vector2 Bend { get; private set; }

        private readonly Transform[] _transforms;
        private readonly SpriteRenderer[] _renderers;
        private readonly Vector3[] _positions;
        private readonly Quaternion[] _rotations;
        private readonly Vector3[] _scales;
        private readonly Sprite[] _sprites;
        private readonly bool[] _shown;

        public RigPose(Transform[] transforms, SpriteRenderer[] renderers)
        {
            _transforms = transforms;
            _renderers = renderers;
            _positions = new Vector3[transforms.Length];
            _rotations = new Quaternion[transforms.Length];
            _scales = new Vector3[transforms.Length];
            _sprites = new Sprite[renderers.Length];
            _shown = new bool[renderers.Length];
        }

        public void Capture(Vector2 bend)
        {
            Bend = bend;
            for (var i = 0; i < _transforms.Length; i++)
            {
                _positions[i] = _transforms[i].localPosition;
                _rotations[i] = _transforms[i].localRotation;
                _scales[i] = _transforms[i].localScale;
            }

            for (var i = 0; i < _renderers.Length; i++)
            {
                _sprites[i] = _renderers[i].sprite;
                _shown[i] = _renderers[i].enabled;
            }
        }

        public void Apply()
        {
            for (var i = 0; i < _transforms.Length; i++)
                _transforms[i].SetLocalPositionAndRotation(_positions[i], _rotations[i]);

            for (var i = 0; i < _transforms.Length; i++)
                _transforms[i].localScale = _scales[i];

            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sprite = _sprites[i];
                _renderers[i].enabled = _shown[i];
            }
        }
    }
}
