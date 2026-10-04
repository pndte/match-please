using System;
using JetBrains.Diagnostics;
using UnityEngine;

namespace Bw.Entities.Infrastructure
{
    public sealed class UnityLogFactory : ILogFactory
    {
        public ILog GetLog(string category) => new UnityLog(category);
    }

    internal sealed class UnityLog : ILog
    {
        public string Category { get; }

        public UnityLog(string category)
        {
            Category = category;
        }

        public bool IsEnabled(LoggingLevel level) =>
            level is LoggingLevel.FATAL or LoggingLevel.ERROR or LoggingLevel.WARN;

        public void Log(LoggingLevel level, string message, Exception exception)
        {
            if (!IsEnabled(level))
                return;

            if (level == LoggingLevel.WARN)
            {
                Debug.LogWarning(exception == null ? message : $"{message}\n{exception}");
                return;
            }

            if (!string.IsNullOrEmpty(message))
                Debug.LogError(message);

            if (exception != null)
                Debug.LogException(exception);
        }
    }
}
