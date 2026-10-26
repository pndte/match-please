using System;
using System.Linq;
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

        [Test]
        public void SettingsInstallersOfTheProjectContextResolveEveryBinding()
        {
            var project = _containers.Project();

            Assert.DoesNotThrow(project.ResolveRoots);
        }

        [Test]
        public void ProjectContextRunsOnlyTheInstallersTheValidationStandsIn()
        {
            var context = AssetDatabase.LoadAssetAtPath<GameObject>(ValidationContainers.ProjectContextPrefab).GetComponent<ProjectContext>();

            var installers = context.Installers.Select(installer => installer.GetType()).ToList();

            Assert.That(installers, Is.EquivalentTo(new[] { typeof(AddressableSettingsInstaller), typeof(ProjectInstaller) }));
            Assert.That(context.InstallerPrefabs, Is.Empty);
            Assert.That(context.ScriptableObjectInstallers, Is.Empty, "the project context references no settings asset: they load through Addressables, or the scene bundles duplicate them");
        }

        [Test]
        public void ProjectContextInstallsSomeSettings()
        {
            var settings = ValidationContainers.SettingsInstallers();

            Assert.That(settings, Is.Not.Empty);
            Assert.That(settings.Select(installer => installer.GetType()).Distinct().Count(), Is.EqualTo(settings.Count), "an installer is listed twice");
        }
    }
}
