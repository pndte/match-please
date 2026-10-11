using DG.Tweening;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class MaterialChannel
    {
        public float Value { get; private set; }

        private readonly Material _material;
        private readonly int _property;
        private readonly GameObject _owner;

        public MaterialChannel(Material material, int property, GameObject owner)
        {
            _material = material;
            _property = property;
            _owner = owner;
        }

        public void Set(float value)
        {
            DOTween.Kill(this);
            Write(value);
        }

        public Tween To(float value, float seconds)
        {
            DOTween.Kill(this);
            return DOTween.To(() => Value, Write, value, seconds)
                .SetTarget(this)
                .SetLink(_owner);
        }

        private void Write(float value)
        {
            Value = value;
            _material.SetFloat(_property, value);
        }
    }
}
