using NUnit.Framework;
using UnityEngine;

using DreamVille.App;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.App
{
    public class PauseServiceTests
    {
        private FakePlayerControlSwitch _playerControl;
        private FakeCursorService _cursor;
        private PauseService _service;

        [SetUp]
        public void SetUp()
        {
            _playerControl = new FakePlayerControlSwitch();
            _cursor = new FakeCursorService();
            _service = new PauseService(_playerControl, _cursor);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Pause_DisablesPlayerControlAndSwitchesCursorToUi()
        {
            _service.Pause();

            Assert.That(_service.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(_playerControl.DisableCalls, Is.EqualTo(1));
            Assert.That(_cursor.LastMode, Is.EqualTo(FakeCursorService.Mode.Ui));
        }

        [Test]
        public void Resume_RestoresTimeScaleAndPlayerControl()
        {
            _service.Pause();

            _service.Resume();

            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(_service.IsPaused, Is.False);
            Assert.That(_playerControl.EnableCalls, Is.EqualTo(1));
            Assert.That(_playerControl.IsEnabled, Is.True);
            Assert.That(_cursor.LastMode, Is.EqualTo(FakeCursorService.Mode.Gameplay));
        }

        [Test]
        public void SetPlayerControl_False_DoesNotChangeTimeScale()
        {
            Time.timeScale = 1f;

            _service.SetPlayerControl(false);

            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(_playerControl.DisableCalls, Is.EqualTo(1));
            Assert.That(_cursor.LastMode, Is.EqualTo(FakeCursorService.Mode.Ui));
        }
    }
}
