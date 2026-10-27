using System;
using System.Text;

namespace Bw.Entities.Network
{
    public static class LevelQuery
    {
        private const string Question = "level?";
        private const string AnswerPrefix = "level:";

        public static byte[] Payload => Encoding.UTF8.GetBytes(Question);

        public static bool IsAsked(byte[] payload) =>
            Encoding.UTF8.GetString(payload) == Question;

        public static string Answer(string level) =>
            AnswerPrefix + level;

        public static bool TryReadAnswer(string reason, out string level)
        {
            level = default;
            if (string.IsNullOrEmpty(reason) || !reason.StartsWith(AnswerPrefix, StringComparison.Ordinal) || reason.Length == AnswerPrefix.Length)
                return false;

            level = reason.Substring(AnswerPrefix.Length);
            return true;
        }
    }
}
