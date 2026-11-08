using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.Gameplay.Environment;

namespace DreamVille.Tests.EditMode.Environment
{
    /// <summary>
    /// The Level_01 balloons keep their drift tuning after the
    /// <see cref="RandomFloating"/> field renames, and no balloon is steered by two components again.
    /// </summary>
    public class FloatingPropsContentTests
    {
        private const string Level01ScenePath = "Assets/_Project/Scenes/Level_01.unity";
        private const int BalloonCount = 14;

        private string _originalScenePath;
        private bool _scenesOpened;
        private RandomFloating[] _floatingProps;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            _originalScenePath = activeScene.path;

            // Interactively a dirty scene just skips the fixture; on a build machine it is a real failure.
            const string dirtySceneMessage = "A dirty scene is open; save or discard changes before running this fixture.";
            if (Application.isBatchMode)
                Assert.That(activeScene.isDirty, Is.False, dirtySceneMessage);
            else
                Assume.That(!activeScene.isDirty, dirtySceneMessage);

            _scenesOpened = true;

            EditorSceneManager.OpenScene(Level01ScenePath, OpenSceneMode.Single);
            _floatingProps = Object.FindObjectsByType<RandomFloating>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Never reopen a scene when the setup was skipped: that would discard the user's unsaved changes.
            if (!_scenesOpened)
                return;

            if (string.IsNullOrEmpty(_originalScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else
                EditorSceneManager.OpenScene(_originalScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void Level01Balloons_KeepFloatingTuning()
        {
            Assert.That(_floatingProps, Has.Length.EqualTo(BalloonCount), "Level_01 floating balloon count.");

            foreach (RandomFloating prop in _floatingProps)
            {
                var serialized = new SerializedObject(prop);

                Assert.That(serialized.FindProperty("_radius").floatValue, Is.EqualTo(20f), $"'{prop.name}' radius.");
                Assert.That(serialized.FindProperty("_stopTime").floatValue, Is.EqualTo(1f), $"'{prop.name}' stop time.");
                Assert.That(serialized.FindProperty("_minY").floatValue, Is.EqualTo(-50f), $"'{prop.name}' min height.");
                Assert.That(serialized.FindProperty("_maxY").floatValue, Is.EqualTo(50f), $"'{prop.name}' max height.");
            }
        }

        [Test]
        public void Level01FloatingProps_AtMostOnePerGameObject()
        {
            string[] doubled = _floatingProps
                .GroupBy(prop => prop.gameObject)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key.name)
                .ToArray();

            Assert.That(doubled, Is.Empty, "Two RandomFloating components on one object fight over its position.");
        }
    }
}
