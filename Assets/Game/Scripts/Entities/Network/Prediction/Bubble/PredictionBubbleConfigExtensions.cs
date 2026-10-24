using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public static class PredictionBubbleConfigExtensions
    {
        public static bool Covers(this PredictionBubbleConfig config, Vector2 center, Vector2 position) => //TODO: сервер не знает гистерезиса клиента и берёт радиус выхода: объект между EnterRadius и LeaveRadius, который клиент ещё не начал предсказывать, сервер считает предсказанным
            (position - center).sqrMagnitude <= config.LeaveRadius * config.LeaveRadius;
    }
}
