using Bw.Entities.Network.Ticks;
using Bw.Injection.Network;
using NUnit.Framework;
using UnityEditor;

namespace Bw.Tests.Entities.Network.Ticks
{
    public sealed class NetworkTicksConfigTests
    {
        private const string SettingsPath = "Assets/Resources/Setup/Network/NetworkSettings.asset";

        [Test]
        public void DefaultsKeepTheirBoundsInOrder()
        {
            var config = new NetworkTicksConfig();

            Assert.That(config.MinLeadTicks, Is.LessThanOrEqualTo(config.InitialLeadTicks));
            Assert.That(config.InitialLeadTicks, Is.LessThanOrEqualTo(config.MaxLeadTicks));
            Assert.That(config.MinInputDelayTicks, Is.LessThanOrEqualTo(config.MaxInputDelayTicks));
            Assert.That(config.MaxLeadTicks + config.MaxInputDelayTicks, Is.LessThanOrEqualTo(config.MaxBufferedInputTicks));
        }

        [Test]
        public void ProjectSettingsKeepTheirBoundsInOrder()
        {
            var settings = new SerializedObject(AssetDatabase.LoadAssetAtPath<NetworkSettingsInstaller>(SettingsPath));

            float Float(string field) => settings.FindProperty("_ticks." + field).floatValue;
            int Int(string field) => settings.FindProperty("_ticks." + field).intValue;

            Assert.That(Float(nameof(NetworkTicksConfig.MinLeadTicks)), Is.LessThanOrEqualTo(Float(nameof(NetworkTicksConfig.InitialLeadTicks))));
            Assert.That(Float(nameof(NetworkTicksConfig.InitialLeadTicks)), Is.LessThanOrEqualTo(Float(nameof(NetworkTicksConfig.MaxLeadTicks))));
            Assert.That(Int(nameof(NetworkTicksConfig.MinInputDelayTicks)), Is.LessThanOrEqualTo(Int(nameof(NetworkTicksConfig.MaxInputDelayTicks))));
            Assert.That(
                Float(nameof(NetworkTicksConfig.MaxLeadTicks)) + Int(nameof(NetworkTicksConfig.MaxInputDelayTicks)),
                Is.LessThanOrEqualTo(Int(nameof(NetworkTicksConfig.MaxBufferedInputTicks))));
        }
    }
}
