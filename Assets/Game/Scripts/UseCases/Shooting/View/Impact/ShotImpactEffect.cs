using DG.Tweening;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact
{
    public sealed class ShotImpactEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _effect;

        public void Play(Vector2 point, Vector2 direction, float delay)
        {
            var rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
            var effect = Instantiate(_effect, point, rotation); //TODO: пул эффектов попаданий, сейчас каждый попавший выстрел создаёт и удаляет объект
            DOVirtual.DelayedCall(delay, () => effect.Play(), false).SetLink(effect.gameObject);
        }
    }
}
