using Bw.Entities.Content;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Doubles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Bw.Tests.Entities.Content
{
    public sealed class ContentAssetTests
    {
        private TestContentAsset _asset;

        [SetUp]
        public void SetUp() =>
            _asset = ScriptableObject.CreateInstance<TestContentAsset>();

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_asset);

        [Test]
        public void NewAssetHasKeyZero()
        {
            Assert.That(_asset.Key, Is.EqualTo(0));
        }

        [Test]
        public void KeyFieldNamesTheSerializedKey()
        {
            Assert.That(new SerializedObject(_asset).FindProperty(ContentAsset.KeyField), Is.Not.Null);
        }

        [TestCase(42)]
        [TestCase(-7)]
        [TestCase(int.MaxValue)]
        public void KeyIsTheSerializedValue(int key)
        {
            _asset.With(ContentAsset.KeyField, key);

            Assert.That(_asset.Key, Is.EqualTo(key));
        }
    }
}
