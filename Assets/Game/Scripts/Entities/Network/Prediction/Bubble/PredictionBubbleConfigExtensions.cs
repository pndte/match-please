using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public static class PredictionBubbleConfigExtensions
    {
        public static bool NeedsInputs(this PredictionBubbleConfig config, Vector2 center, Vector2 position) //TODO: запас покрывает расхождение показанных клиентом позиций с текущими серверными при беге (опережение и задержка интерполяции, 13 тиков по 0,43 м); при более быстром движении (отлёт от удара) клиент на краю пузыря снова может предсказывать персонажа без его ввода
        {
            var radius = config.LeaveRadius + config.InputRelayMargin;
            return (position - center).sqrMagnitude <= radius * radius;
        }
    }
}
