using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.UI;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// The main-menu logos keep fading after <see cref="ImageFadeOut"/> moved to the UI
    /// assembly and its image field was renamed (same script GUID, same values).
    /// </summary>
    public class MainMenuContentTests
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const int LogoCount = 3;

        private string _originalScenePath;
        private bool _scenesOpened;

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
        public void MainMenuLogos_FadeAfterTwoSeconds()
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            ImageFadeOut[] logos = Object.FindObjectsByType<ImageFadeOut>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            Assert.That(logos, Has.Length.EqualTo(LogoCount), "MainMenu logo fade count.");

            foreach (ImageFadeOut logo in logos)
            {
                var serialized = new SerializedObject(logo);

                Assert.That(serialized.FindProperty("_image").objectReferenceValue, Is.Not.Null, $"'{logo.name}' image.");
                Assert.That(serialized.FindProperty("_showDuration").floatValue, Is.EqualTo(2f), $"'{logo.name}' show duration.");
                Assert.That(serialized.FindProperty("_fadeDuration").floatValue, Is.EqualTo(2f), $"'{logo.name}' fade duration.");
            }
        }
    }
}
