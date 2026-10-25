using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bw.Entities.Infrastructure;
using JetBrains.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.Entities.Infrastructure
{
    public sealed class UnityLogFactoryTests
    {
        private List<(LogType Type, string Text)> _logged;
        private ILog _log;

        [SetUp]
        public void SetUp()
        {
            _logged = new List<(LogType Type, string Text)>();
            Application.logMessageReceived += Record;
            _log = new UnityLogFactory().GetLog("Pool");
        }

        [TearDown]
        public void TearDown() =>
            Application.logMessageReceived -= Record;

        [Test]
        public void LogKeepsItsCategory()
        {
            Assert.That(_log.Category, Is.EqualTo("Pool"));
        }

        [TestCase(LoggingLevel.FATAL, true)]
        [TestCase(LoggingLevel.ERROR, true)]
        [TestCase(LoggingLevel.WARN, true)]
        [TestCase(LoggingLevel.INFO, false)]
        [TestCase(LoggingLevel.VERBOSE, false)]
        [TestCase(LoggingLevel.TRACE, false)]
        [TestCase(LoggingLevel.OFF, false)]
        public void OnlyProblemsAreLogged(LoggingLevel level, bool enabled)
        {
            Assert.That(_log.IsEnabled(level), Is.EqualTo(enabled));
        }

        [TestCase(LoggingLevel.INFO)]
        [TestCase(LoggingLevel.VERBOSE)]
        [TestCase(LoggingLevel.TRACE)]
        [TestCase(LoggingLevel.OFF)]
        public void QuietLevelsReachNothing(LoggingLevel level)
        {
            _log.Log(level, "quiet", new InvalidOperationException("quiet"));

            Assert.That(_logged, Is.Empty);
        }

        [Test]
        public void WarningGoesToTheConsoleAsAWarning()
        {
            _log.Log(LoggingLevel.WARN, "careful", null);

            Assert.That(_logged, Is.EqualTo(new[] { (LogType.Warning, "careful") }));
        }

        [Test]
        public void WarningCarriesItsExceptionInOneMessage()
        {
            _log.Log(LoggingLevel.WARN, "careful", new InvalidOperationException("boom"));

            Assert.That(_logged, Has.Count.EqualTo(1));
            Assert.That(_logged[0].Type, Is.EqualTo(LogType.Warning));
            Assert.That(_logged[0].Text, Does.StartWith("careful\n"));
            Assert.That(_logged[0].Text, Does.Contain("boom"));
        }

        [TestCase(LoggingLevel.ERROR)]
        [TestCase(LoggingLevel.FATAL)]
        public void ProblemGoesToTheConsoleAsAnError(LoggingLevel level)
        {
            LogAssert.Expect(LogType.Error, "broken");

            _log.Log(level, "broken", null);

            Assert.That(_logged, Is.EqualTo(new[] { (LogType.Error, "broken") }));
        }

        [Test]
        public void ErrorWithAnExceptionLogsTheMessageAndTheException()
        {
            LogAssert.Expect(LogType.Error, "broken");
            LogAssert.Expect(LogType.Exception, new Regex("boom"));

            _log.Log(LoggingLevel.ERROR, "broken", new InvalidOperationException("boom"));

            Assert.That(_logged, Has.Count.EqualTo(2));
            Assert.That(_logged[0], Is.EqualTo((LogType.Error, "broken")));
            Assert.That(_logged[1].Type, Is.EqualTo(LogType.Exception));
        }

        [TestCase(null)]
        [TestCase("")]
        public void ErrorWithoutAMessageLogsOnlyTheException(string message)
        {
            LogAssert.Expect(LogType.Exception, new Regex("boom"));

            _log.Log(LoggingLevel.ERROR, message, new InvalidOperationException("boom"));

            Assert.That(_logged, Has.Count.EqualTo(1));
            Assert.That(_logged[0].Type, Is.EqualTo(LogType.Exception));
        }

        private void Record(string condition, string stackTrace, LogType type) =>
            _logged.Add((type, condition));
    }
}
