using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public static class PredictionBubbleConfigExtensions
    {
        public static bool Covers(this PredictionBubbleConfig config, Vector2 center, Vector2 position) => //TODO: сервер не знает гистерезиса клиента и берёт радиус выхода: объект между EnterRadius и LeaveRadius, который клиент ещё не начал предсказывать, сервер считает предсказанным
            (position - center).sqrMagnitude <= config.LeaveRadius * config.LeaveRadius;

        public static bool NeedsInputs(this PredictionBubbleConfig config, Vector2 center, Vector2 position) //TODO: запас покрывает расхождение показанных клиентом позиций с текущими серверными при беге и пинге до ~200 мс (опережение и задержка интерполяции, 13 тиков по 0,43 м); при более быстром движении (отлёт от удара) или большем пинге клиент на краю пузыря снова может предсказывать персонажа без его ввода
        {
            var radius = config.LeaveRadius + config.InputRelayMargin;
            return (position - center).sqrMagnitude <= radius * radius;
        }
    }
}
