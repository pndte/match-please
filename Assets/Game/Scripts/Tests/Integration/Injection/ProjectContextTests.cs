using System;
using System.Linq;
using Bw.Entities.Network;
using Bw.Injection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Bw.Tests.Integration.Injection
{
    [Category("Slow")]
    public sealed class ProjectContextTests
    {
        private ValidationContainers _containers;

        [SetUp]
        public void SetUp() =>
            _containers = new ValidationContainers();

        [TearDown]
        public void TearDown() =>
            _containers.Dispose();

        [TestCase(PeerType.Server)]
        [TestCase(PeerType.Client)]
        public void SettingsInstallersOfTheProjectContextResolveEveryBinding(PeerType peer)
        {
            var project = _containers.Project(ValidationContainers.SettingsOf(peer));

            Assert.DoesNotThrow(project.ResolveRoots);
        }

        [Test]
        public void ProjectContextRunsOnlyTheInstallersTheValidationStandsIn()
        {
            var context = AssetDatabase.LoadAssetAtPath<GameObject>(ValidationContainers.ProjectContextPrefab).GetComponent<ProjectContext>();

            var installers = context.Installers.Select(installer => installer.GetType()).ToList();

            Assert.That(installers, Is.EquivalentTo(new[] { typeof(RuntimeSettingsInstaller), typeof(ProjectInstaller) }));
            Assert.That(context.InstallerPrefabs, Is.Empty);
        }

        [Test]
        public void ProjectContextInstallsSomeSettings()
        {
            var context = AssetDatabase.LoadAssetAtPath<GameObject>(ValidationContainers.ProjectContextPrefab).GetComponent<ProjectContext>();

            var settings = context.ScriptableObjectInstallers.ToList();

            Assert.That(settings, Is.Not.Empty);
            Assert.That(settings.Select(installer => installer.GetType()).Distinct().Count(), Is.EqualTo(settings.Count), "an installer is listed twice");
        }
    }
}
