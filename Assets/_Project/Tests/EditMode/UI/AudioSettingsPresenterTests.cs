using NUnit.Framework;

using DreamVille.Tests.EditMode.Fakes;
using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>The volume sliders follow the settings both ways and never echo a change back.</summary>
    public class AudioSettingsPresenterTests
    {
        private FakeAudioSettingsView _view;
        private FakeAudioSettings _settings;
        private AudioSettingsPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeAudioSettingsView();
            _settings = new FakeAudioSettings { MusicVolume = 0.3f, SfxVolume = 0.4f };
            _presenter = new AudioSettingsPresenter(_view, _settings);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
        }

        [Test]
        public void Initialize_ShowsCurrentVolumes()
        {
            _presenter.Initialize();

            Assert.That(_view.MusicShown, Is.EqualTo(new[] { 0.3f }));
            Assert.That(_view.SfxShown, Is.EqualTo(new[] { 0.4f }));
        }

        [Test]
        public void MusicVolumeEdited_WritesMusicVolume()
        {
            _presenter.Initialize();

            _view.EditMusicVolume(0.5f);

            Assert.That(_settings.MusicWrites, Is.EqualTo(new[] { 0.5f }));
            Assert.That(_settings.SfxWrites, Is.Empty);
        }

        [Test]
        public void MusicVolumeEdited_ServiceRaisesChanged_WritesOnce()
        {
            _presenter.Initialize();

            _view.EditMusicVolume(0.5f);

            Assert.That(_settings.MusicWrites, Is.EqualTo(new[] { 0.5f }));
            Assert.That(_view.MusicShown, Is.EqualTo(new[] { 0.3f, 0.5f }));
        }

        [Test]
        public void Dispose_ThenEvents_DoNothing()
        {
            _presenter.Initialize();
            _presenter.Dispose();

            _view.EditMusicVolume(0.5f);
            _settings.ChangeSfxVolumeElsewhere(0.9f);

            Assert.That(_settings.MusicWrites, Is.Empty);
            Assert.That(_view.SfxShown, Is.EqualTo(new[] { 0.4f }));
        }

        [Test]
        public void SfxVolumeEdited_WritesSfxVolume()
        {
            _presenter.Initialize();

            _view.EditSfxVolume(0.6f);

            Assert.That(_settings.SfxWrites, Is.EqualTo(new[] { 0.6f }));
            Assert.That(_settings.MusicWrites, Is.Empty);
        }

        [Test]
        public void VolumeChangedElsewhere_UpdatesViewWithoutWritingBack()
        {
            _presenter.Initialize();

            _settings.ChangeMusicVolumeElsewhere(0.7f);
            _settings.ChangeSfxVolumeElsewhere(0.2f);

            Assert.That(_view.MusicShown, Is.EqualTo(new[] { 0.3f, 0.7f }));
            Assert.That(_view.SfxShown, Is.EqualTo(new[] { 0.4f, 0.2f }));
            Assert.That(_settings.MusicWrites, Is.Empty);
            Assert.That(_settings.SfxWrites, Is.Empty);
        }
    }
}
