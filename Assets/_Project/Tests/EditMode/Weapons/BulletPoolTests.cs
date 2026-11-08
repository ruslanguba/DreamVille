using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Gameplay.Weapons.Bullets;

namespace DreamVille.Tests.EditMode.Weapons
{
    /// <summary><see cref="BulletPool"/> reuses a returned bullet exactly once, even when it is returned twice.</summary>
    public class BulletPoolTests
    {
        private GameObject _poolObject;
        private GameObject _template;
        private BulletPool _pool;

        [SetUp]
        public void SetUp()
        {
            _template = new GameObject("BulletTemplate");
            Bullet template = _template.AddComponent<Bullet>();
            _poolObject = new GameObject("BulletPool");
            _pool = _poolObject.AddComponent<BulletPool>();
            var serializedPool = new SerializedObject(_pool);
            serializedPool.FindProperty("_prefab").objectReferenceValue = template;
            serializedPool.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Bullet created in Object.FindObjectsByType<Bullet>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(created.gameObject);

            Object.DestroyImmediate(_poolObject);
        }

        [Test]
        public void Return_Twice_KeepsOneEntry()
        {
            Bullet bullet = _pool.Get();
            _pool.Return(bullet);
            _pool.Return(bullet);

            Bullet reused = _pool.Get();

            Assert.That(reused, Is.SameAs(bullet));
            Assert.That(reused.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void Get_AfterADoubleReturn_GivesTwoInstances()
        {
            Bullet bullet = _pool.Get();
            _pool.Return(bullet);
            _pool.Return(bullet);

            Bullet first = _pool.Get();
            Bullet second = _pool.Get();

            Assert.That(second, Is.Not.SameAs(first));
        }
    }
}
