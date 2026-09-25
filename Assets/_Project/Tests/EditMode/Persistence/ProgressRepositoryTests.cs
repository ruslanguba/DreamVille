using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Persistence;

namespace DreamVille.Tests.EditMode.Persistence
{
    public class ProgressRepositoryTests
    {
        private const string LevelId = "__test_level";

        private InMemorySaveStorage _storage;
        private ProgressRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemorySaveStorage();
            _repository = new ProgressRepository(_storage);
        }

        [Test]
        public void GetStars_UnknownLevel_ReturnsZero()
        {
            int stars = _repository.GetStars(LevelId);

            Assert.That(stars, Is.EqualTo(0));
        }

        [Test]
        public void SetStarsIfHigher_WhenHigher_StoresAndFlushes()
        {
            bool written = _repository.SetStarsIfHigher(LevelId, 2);

            Assert.That(written, Is.True);
            Assert.That(_repository.GetStars(LevelId), Is.EqualTo(2));
            Assert.That(_storage.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void SetStarsIfHigher_WhenLowerOrEqual_KeepsStoredValueAndReturnsFalse()
        {
            _repository.SetStarsIfHigher(LevelId, 2);

            bool written = _repository.SetStarsIfHigher(LevelId, 1);

            Assert.That(written, Is.False);
            Assert.That(_repository.GetStars(LevelId), Is.EqualTo(2));
        }

        [Test]
        public void MarkCompleted_MakesIsCompletedTrue()
        {
            _repository.MarkCompleted(LevelId);

            Assert.That(_repository.IsCompleted(LevelId), Is.True);
        }

        [Test]
        public void IsCompleted_UnknownLevel_ReturnsFalse()
        {
            bool completed = _repository.IsCompleted(LevelId);

            Assert.That(completed, Is.False);
        }

        [Test]
        public void TrySetBestScore_WhenBetter_StoresAndReturnsTrue()
        {
            bool written = _repository.TrySetBestScore(LevelId, 100f);

            Assert.That(written, Is.True);
            Assert.That(_repository.GetBestScore(LevelId), Is.EqualTo(100f));
        }

        [Test]
        public void TrySetBestScore_WhenNotBetter_LeavesStoredValue()
        {
            _repository.TrySetBestScore(LevelId, 100f);

            bool written = _repository.TrySetBestScore(LevelId, 50f);

            Assert.That(written, Is.False);
            Assert.That(_repository.GetBestScore(LevelId), Is.EqualTo(100f));
        }

        [Test]
        public void GetUnlockedPillows_WhenAbsent_ReturnsZero()
        {
            int unlocked = _repository.GetUnlockedPillows();

            Assert.That(unlocked, Is.EqualTo(0));
        }

        [Test]
        public void SetUnlockedPillowsIfHigher_StoresHigherIgnoresLowerAndFlushes()
        {
            bool higherWritten = _repository.SetUnlockedPillowsIfHigher(2);
            bool lowerWritten = _repository.SetUnlockedPillowsIfHigher(1);

            Assert.That(higherWritten, Is.True);
            Assert.That(lowerWritten, Is.False);
            Assert.That(_repository.GetUnlockedPillows(), Is.EqualTo(2));
            Assert.That(_storage.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void Write_WithEmptyLevelId_DoesNotTouchStorage()
        {
            LogAssert.Expect(LogType.Error, new Regex(".*"));

            bool written = _repository.SetStarsIfHigher(string.Empty, 3);

            Assert.That(written, Is.False);
            Assert.That(_storage.SaveCount, Is.EqualTo(0));
        }
    }
}
