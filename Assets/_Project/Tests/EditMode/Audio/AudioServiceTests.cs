using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Audio
{
    /// <summary>
    /// <see cref="AudioService"/> against hand-written doubles: the mixer/sources behind
    /// <see cref="FakeAudioOutput"/>, an in-memory save and a <see cref="FakeActiveSceneTracker"/>
    /// the test drives, so no editor scene and no engine event is involved.
    /// </summary>
    public class AudioServiceTests
    {
        private const string BootScene = "AudioServiceTests_Boot";
        private const string MenuScene = "AudioServiceTests_Menu";
        private const string LevelScene = "AudioServiceTests_Level";
        private const string UnknownScene = "AudioServiceTests_NoSuchScene";

        private static readonly Regex NotInCatalogWarning = new Regex(@"\[AudioService\].*not in the catalog");

        private readonly List<UnityEngine.Object> _createdObjects = new List<UnityEngine.Object>();

        private FakeAudioOutput _output;
        private InMemorySaveStorage _storage;
        private AudioSettingsRepository _settings;
        private FakeActiveSceneTracker _scenes;
        private AudioConfig _config;
        private AudioClip _levelClip;
        private SceneCatalog _catalog;
        private AudioService _service;

        [SetUp]
        public void SetUp()
        {
            _output = new FakeAudioOutput();
            _storage = new InMemorySaveStorage();
            _settings = new AudioSettingsRepository(_storage);
            _scenes = new FakeActiveSceneTracker(BootScene);
            _config = Track(AudioConfig.CreateForTests(0.3f));

            _levelClip = Track(AudioClip.Create("level-music", 1, 1, 8000, false));
            SceneDefinition boot = Track(SceneDefinition.CreateForTests(BootScene));
            SceneDefinition menu = Track(SceneDefinition.CreateForTests(MenuScene));
            LevelDefinition level = Track(LevelDefinition.CreateForTests(LevelScene, "lvl", music: _levelClip));
            _catalog = Track(SceneCatalog.CreateForTests(boot, menu, level));

            _service = new AudioService(_output, _settings, _scenes, _catalog, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();

            foreach (UnityEngine.Object created in _createdObjects)
                UnityEngine.Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        [Test]
        public void Initialize_WithoutSavedVolumes_AppliesConfigDefaultToBothChannels()
        {
            _service.Initialize();

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

            _service.Initialize();

            Assert.That(_service.MusicVolume, Is.EqualTo(0.7f));
            Assert.That(_service.SfxVolume, Is.EqualTo(0.9f));
            Assert.That(_output.MusicVolume, Is.EqualTo(0.7f));
            Assert.That(_output.SfxVolume, Is.EqualTo(0.9f));
        }

        [Test]
        public void Initialize_ActiveSceneWithoutMusic_StaysSilent()
        {
            _service.Initialize();

            Assert.That(_output.LastMusicClip, Is.Null);
        }

        [Test]
        public void Initialize_ActiveSceneIsALevel_PlaysItsMusicWithoutWaitingForALoad()
        {
            _scenes.Load(LevelScene);

            _service.Initialize();

            Assert.That(_output.LastMusicClip, Is.SameAs(_levelClip));
            Assert.That(_output.LastMusicLoop, Is.True);
        }

        [Test]
        public void SetMusicVolume_AppliesToMixerAndWritesToRepository()
        {
            _service.SetMusicVolume(0.6f);

            Assert.That(_output.MusicVolume, Is.EqualTo(0.6f));
            Assert.That(_settings.TryGetMusicVolume(out float stored), Is.True);
            Assert.That(stored, Is.EqualTo(0.6f));
        }

        [Test]
        public void PlaySfx_ForwardsTheClip()
        {
            AudioClip clip = Track(AudioClip.Create("sfx", 1, 1, 8000, false));

            _service.PlaySfx(clip);

            Assert.That(_output.LastSfxClip, Is.SameAs(clip));
        }

        [Test]
        public void SetMusicVolume_RaisesMusicVolumeChanged()
        {
            float? raised = null;
            _service.MusicVolumeChanged += value => raised = value;

            _service.SetMusicVolume(0.2f);

            Assert.That(raised, Is.EqualTo(0.2f));
        }

        [Test]
        public void SetSfxVolume_PersistsTheValue()
        {
            _service.SetSfxVolume(0.8f);

            Assert.That(_settings.TryGetSfxVolume(out float stored), Is.True);
            Assert.That(stored, Is.EqualTo(0.8f));
        }

        [Test]
        public void SetSfxVolume_RaisesSfxVolumeChanged()
        {
            float? raised = null;
            _service.SfxVolumeChanged += value => raised = value;

            _service.SetSfxVolume(0.4f);

            Assert.That(raised, Is.EqualTo(0.4f));
        }

        [Test]
        public void SceneLoaded_KnownLevel_PlaysTheCatalogClip()
        {
            _service.Initialize();

            _scenes.Load(LevelScene);

            Assert.That(_output.LastMusicClip, Is.SameAs(_levelClip));
            Assert.That(_output.LastMusicLoop, Is.True);
        }

        [Test]
        public void SceneLoaded_SceneWithoutMusic_KeepsCurrentMusic()
        {
            _service.Initialize();
            _scenes.Load(LevelScene);

            _scenes.Load(MenuScene);

            Assert.That(_output.LastMusicClip, Is.SameAs(_levelClip));
        }

        [Test]
        public void SceneLoaded_UnknownScene_KeepsCurrentMusicAndWarns()
        {
            _service.Initialize();
            _scenes.Load(LevelScene);

            LogAssert.Expect(LogType.Warning, NotInCatalogWarning);
            _scenes.Load(UnknownScene);

            Assert.That(_output.LastMusicClip, Is.SameAs(_levelClip));
        }

        [Test]
        public void Dispose_FlushesSettingsOnce()
        {
            _service.Initialize();

            _service.Dispose();

            Assert.That(_storage.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_StopsListeningToSceneLoads()
        {
            _service.Initialize();
            _service.Dispose();

            _scenes.Load(LevelScene);

            Assert.That(_output.LastMusicClip, Is.Null);
        }

        private T Track<T>(T asset) where T : UnityEngine.Object
        {
            _createdObjects.Add(asset);
            return asset;
        }
    }
}
