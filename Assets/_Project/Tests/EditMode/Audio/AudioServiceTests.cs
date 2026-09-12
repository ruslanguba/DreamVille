using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Audio
{
    /// <summary>
    /// <see cref="AudioService"/>'s reaction to <c>SceneManager.sceneLoaded</c> is exercised
    /// through its private <c>PlaySceneMusic(string)</c> helper via reflection, because
    /// constructing a real <see cref="Scene"/> with an arbitrary name requires Play mode.
    /// <see cref="AudioService.Initialize"/> also plays the music of whatever scene happens to
    /// be active in the editor while the fixture runs; <see cref="InitializeService"/> keeps
    /// that side effect from leaking into every test (see its own summary).
    /// </summary>
    public class AudioServiceTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new List<UnityEngine.Object>();

        private FakeAudioOutput _output;
        private InMemorySaveStorage _storage;
        private AudioSettingsRepository _settings;
        private AudioConfig _config;
        private AudioService _service;

        [SetUp]
        public void SetUp()
        {
            _output = new FakeAudioOutput();
            _storage = new InMemorySaveStorage();
            _settings = new AudioSettingsRepository(_storage);
            _config = Track(AudioConfig.CreateForTests(0.3f));
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();

            foreach (UnityEngine.Object created in _createdObjects)
                UnityEngine.Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        [Test]
        public void Initialize_WithoutSavedVolumes_AppliesConfigDefaultToBothChannels()
        {
            _service = CreateService(CreateCatalogForActiveScene());

            InitializeService();

            Assert.That(_service.MusicVolume, Is.EqualTo(0.3f));
            Assert.That(_service.SfxVolume, Is.EqualTo(0.3f));
            Assert.That(_output.MusicVolume, Is.EqualTo(0.3f));
            Assert.That(_output.SfxVolume, Is.EqualTo(0.3f));
        }

        [Test]
        public void Initialize_WithSavedVolumes_PrefersSavedValues()
        {
            _settings.SetMusicVolume(0.7f);
            _settings.SetSfxVolume(0.9f);
            _service = CreateService(CreateCatalogForActiveScene());

            InitializeService();

            Assert.That(_service.MusicVolume, Is.EqualTo(0.7f));
            Assert.That(_service.SfxVolume, Is.EqualTo(0.9f));
            Assert.That(_output.MusicVolume, Is.EqualTo(0.7f));
            Assert.That(_output.SfxVolume, Is.EqualTo(0.9f));
        }

        [Test]
        public void SetMusicVolume_AppliesToMixerAndWritesToRepository()
        {
            _service = CreateService(CreateCatalogForActiveScene());

            _service.SetMusicVolume(0.6f);

            Assert.That(_output.MusicVolume, Is.EqualTo(0.6f));
            Assert.That(_settings.TryGetMusicVolume(out float stored), Is.True);
            Assert.That(stored, Is.EqualTo(0.6f));
        }

        [Test]
        public void SetSfxVolume_RaisesSfxVolumeChanged()
        {
            _service = CreateService(CreateCatalogForActiveScene());
            float? raised = null;
            _service.SfxVolumeChanged += value => raised = value;

            _service.SetSfxVolume(0.4f);

            Assert.That(raised, Is.EqualTo(0.4f));
        }

        [Test]
        public void SceneLoaded_KnownLevel_PlaysTheCatalogClip()
        {
            AudioClip clip = Track(AudioClip.Create("x", 1, 1, 8000, false));
            LevelDefinition level = Track(LevelDefinition.CreateForTests("AudioServiceTests_KnownLevel", "lvl", music: clip));
            _service = CreateService(CreateCatalogForActiveScene(level));
            InitializeService();

            InvokePlaySceneMusic(_service, "AudioServiceTests_KnownLevel");

            Assert.That(_output.LastMusicClip, Is.SameAs(clip));
            Assert.That(_output.LastMusicLoop, Is.True);
        }

        [Test]
        public void SceneLoaded_UnknownScene_KeepsCurrentMusicAndWarns()
        {
            AudioClip clip = Track(AudioClip.Create("x", 1, 1, 8000, false));
            LevelDefinition level = Track(LevelDefinition.CreateForTests("AudioServiceTests_KnownLevel", "lvl", music: clip));
            _service = CreateService(CreateCatalogForActiveScene(level));
            InitializeService();
            InvokePlaySceneMusic(_service, "AudioServiceTests_KnownLevel");

            LogAssert.Expect(LogType.Warning, new Regex(".*"));
            InvokePlaySceneMusic(_service, "AudioServiceTests_NoSuchScene");

            Assert.That(_output.LastMusicClip, Is.SameAs(clip));
        }

        [Test]
        public void Dispose_FlushesSettingsOnce()
        {
            _service = CreateService(CreateCatalogForActiveScene());
            InitializeService();

            _service.Dispose();

            Assert.That(_storage.SaveCount, Is.EqualTo(1));
        }

        private AudioService CreateService(SceneCatalog catalog) => new AudioService(_output, _settings, catalog, _config);

        /// <summary>
        /// Calls <see cref="AudioService.Initialize"/>. When the editor's active scene is an
        /// unsaved, unnamed one (its name is empty), <c>PlaySceneMusic</c> cannot find it in any
        /// catalog and logs a warning; that side effect is a property of the environment the
        /// test happens to run in, not of the behaviour under test, so it is consumed here
        /// instead of leaking into every caller.
        /// </summary>
        private void InitializeService()
        {
            if (string.IsNullOrEmpty(SceneManager.GetActiveScene().name))
                LogAssert.Expect(LogType.Warning, new Regex(".*"));

            _service.Initialize();
        }

        private SceneCatalog CreateCatalogForActiveScene(params LevelDefinition[] levels)
        {
            SceneDefinition activeScene = Track(SceneDefinition.CreateForTests(SceneManager.GetActiveScene().name));
            SceneDefinition menu = Track(SceneDefinition.CreateForTests("AudioServiceTests_Menu"));

            return Track(SceneCatalog.CreateForTests(activeScene, menu, levels));
        }

        /// <summary>
        /// Invokes the private helper <c>AudioService.PlaySceneMusic(string)</c>, the seam
        /// both <c>Initialize</c> and the <c>sceneLoaded</c> handler funnel through.
        /// </summary>
        private static void InvokePlaySceneMusic(AudioService service, string sceneName)
        {
            MethodInfo method = typeof(AudioService).GetMethod("PlaySceneMusic", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null,
                $"{nameof(AudioService)} no longer exposes a private PlaySceneMusic(string) helper; this test's seam has moved.");

            method.Invoke(service, new object[] { sceneName });
        }

        private T Track<T>(T asset) where T : UnityEngine.Object
        {
            _createdObjects.Add(asset);
            return asset;
        }
    }
}
