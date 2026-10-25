using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bw.Tests.Integration.Architecture.CodeStyle;
using NUnit.Framework;

namespace Bw.Tests.Integration.Architecture
{
    [Category("Slow")]
    public sealed class CodeStyleTests
    {
        private const string Scripts = "Assets/Game/Scripts";

        //TODO: старый долг: к концу блока приклеена следующая строка; переформатировать только по слову автора и убрать запись отсюда
        private static readonly HashSet<string> KnownGluedBlocks = new()
        {
            "Entities/Pool/GameObjects/PrefabPools.cs: if (pool.InUseCap == null)",
            "Entities/Pool/GameObjects/PrefabPools.cs: if (!listed.Add(pool.Prefab))",
            "Entities/Pool/GameObjects/PrefabPools.cs: if (pool.ViewOnly && !withViews)",
            "Entities/Utilities/BwViewableList.cs: _myStorage.Add(item);",
            "Setup/NetworkAutoStart.cs: #endif",
            "UseCases/Camera/View/Shake/CameraShake.cs: _lastPosition = position;",
        };

        //TODO: старый долг: конструкторы больше чем на шесть аргументов; разбирать по ролям только по слову автора и убрать запись отсюда
        private static readonly HashSet<string> KnownLongConstructors = new()
        {
            "Entities/Network/Prediction/Events/OutcomeDispatcher.cs: OutcomeDispatcher",
            "Entities/Network/Prediction/Events/OutcomeReceiver.cs: OutcomeReceiver",
            "Entities/Network/Prediction/Stream/PredictionInputSender.cs: PredictionInputSender",
            "Entities/Network/Variables/Table/NetVariablesTableServer.cs: NetVariablesTableServer",
            "UseCases/Character/Network/TimelineBlendView.cs: TimelineBlendView",
            "UseCases/Combat/Shooting/Network/UnheldWeaponDespawner.cs: UnheldWeaponDespawner",
            "UseCases/Combat/Weapon/Network/Prediction/HitPredictor.cs: HitPredictor",
            "UseCases/Combat/Weapon/Network/RaycastShooter.cs: RaycastShooter",
            "UseCases/FogOfWar/View/Fog/FogCover.cs: FogCover",
            "UseCases/FogOfWar/View/FogOfWarView.cs: FogOfWarView",
            "UseCases/Mortality/Network/DeadDespawner.cs: DeadDespawner",
            "UseCases/Movement/Network/Prediction/SmoothedStateView.cs: SmoothedStateView",
            "UseCases/Spawning/Network/NetworkCharactersSpawner.cs: NetworkCharactersSpawner",
        };

        private List<string> _sources;
        private Dictionary<string, string> _gluedBlocks;
        private List<string> _sharedLines;
        private Dictionary<string, string> _longConstructors;

        [OneTimeSetUp]
        public void ReadTheScripts()
        {
            _sources = new List<string>();
            _gluedBlocks = new Dictionary<string, string>();
            _sharedLines = new List<string>();
            _longConstructors = new Dictionary<string, string>();
            foreach (var file in Directory.EnumerateFiles(Scripts, "*.cs", SearchOption.AllDirectories).OrderBy(file => file, StringComparer.Ordinal))
            {
                var path = Path.GetRelativePath(Scripts, file).Replace('\\', '/');
                var lines = File.ReadAllText(file).Replace("\r\n", "\n").Split('\n');
                _sources.Add(path);
                foreach (var (line, text) in BlockSpacing.Violations(lines))
                    _gluedBlocks[$"{path}: {text}"] = $"{path}:{line}: {text}";

                var (shared, constructors) = ParameterColumns.Violations(lines);
                foreach (var (line, name, count) in shared)
                    _sharedLines.Add($"{path}:{line} {name} ({count} parameters)");

                foreach (var (line, name, count) in constructors)
                    _longConstructors[$"{path}: {name}"] = $"{path}:{line} {name} ({count} parameters)";
            }
        }

        [Test]
        public void StyleChecksSeeTheScripts()
        {
            Assert.That(_sources.Count, Is.GreaterThan(1000));
        }

        [Test]
        public void BlocksAreSetOffFromTheNextStatement()
        {
            var glued = _gluedBlocks.Where(found => !KnownGluedBlocks.Contains(found.Key)).Select(found => found.Value);

            Assert.That(glued, Is.Empty, "leave a blank line after the block (block_spacing)");
        }

        [Test]
        public void KnownGluedBlocksAreStillThere()
        {
            Assert.That(KnownGluedBlocks.Where(known => !_gluedBlocks.ContainsKey(known)), Is.Empty, "the block is fixed: remove it from KnownGluedBlocks");
        }

        [Test]
        public void SignaturesOverThreeParametersPutOneParameterPerLine()
        {
            Assert.That(_sharedLines, Is.Empty, "put the parameters one per line (param_columns)");
        }

        [Test]
        public void ConstructorsTakeAtMostSixParameters()
        {
            var tooLong = _longConstructors.Where(found => !KnownLongConstructors.Contains(found.Key)).Select(found => found.Value);

            Assert.That(tooLong, Is.Empty, "split the class by its roles instead of growing the constructor");
        }

        [Test]
        public void KnownLongConstructorsAreStillThere()
        {
            Assert.That(KnownLongConstructors.Where(known => !_longConstructors.ContainsKey(known)), Is.Empty, "the constructor is split: remove it from KnownLongConstructors");
        }
    }
}
