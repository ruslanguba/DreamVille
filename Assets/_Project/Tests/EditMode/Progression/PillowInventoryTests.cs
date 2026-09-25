using System;
using System.Collections.Generic;
using NUnit.Framework;

using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.Progression.Pillows;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>
    /// <see cref="PillowInventory"/> over a real <see cref="ProgressRepository"/> on an in-memory save, with a
    /// catalog shaped like the game's: three pillow-free levels, then the lobby and a level that allow two.
    /// </summary>
    public class PillowInventoryTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new List<UnityEngine.Object>();

        private InMemorySaveStorage _storage;
        private ProgressRepository _progress;
        private LevelDefinition _pillowFreeLevel;
        private LevelDefinition _lobby;
        private LevelDefinition _pillowLevel;
        private SceneCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemorySaveStorage();
            _progress = new ProgressRepository(_storage);

            _pillowFreeLevel = Track(LevelDefinition.CreateForTests("PillowInventoryTests_Level_01", "2", maxPillows: 0));
            _lobby = Track(LevelDefinition.CreateForTests("PillowInventoryTests_Lobby_02", "4", maxPillows: 2));
            _pillowLevel = Track(LevelDefinition.CreateForTests("PillowInventoryTests_Level_03", "5", maxPillows: 2));
            LevelDefinition lobbyOne = Track(LevelDefinition.CreateForTests("PillowInventoryTests_Lobby_01", "1", maxPillows: 0));
            LevelDefinition levelTwo = Track(LevelDefinition.CreateForTests("PillowInventoryTests_Level_02", "3", maxPillows: 0));
            _catalog = Track(SceneCatalog.CreateForTests(null, null, lobbyOne, _pillowFreeLevel, levelTwo, _lobby, _pillowLevel));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (UnityEngine.Object created in _createdObjects)
                UnityEngine.Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        [Test]
        public void Available_IsMinOfUnlockedAndLevelCap()
        {
            LevelDefinition capOne = Track(LevelDefinition.CreateForTests("PillowInventoryTests_CapOne", "99", maxPillows: 1));
            _progress.SetUnlockedPillowsIfHigher(2);

            var cappedInventory = new PillowInventory(_progress, capOne, _catalog);
            var fullInventory = new PillowInventory(_progress, _pillowLevel, _catalog);

            Assert.That(cappedInventory.Available, Is.EqualTo(1));
            Assert.That(fullInventory.Available, Is.EqualTo(2));
        }

        [Test]
        public void UnlockUpTo_Higher_PersistsAndRaisesAvailableChanged()
        {
            var inventory = new PillowInventory(_progress, _lobby, _catalog);
            int? raised = null;
            inventory.AvailableChanged += available => raised = available;

            bool grew = inventory.UnlockUpTo(2);

            Assert.That(grew, Is.True);
            Assert.That(inventory.Available, Is.EqualTo(2));
            Assert.That(raised, Is.EqualTo(2));
            Assert.That(_progress.GetUnlockedPillows(), Is.EqualTo(2));
        }

        [Test]
        public void UnlockUpTo_SameOrLower_ReturnsFalseWithoutEvent()
        {
            var inventory = new PillowInventory(_progress, _lobby, _catalog);
            inventory.UnlockUpTo(2);
            int raisedCount = 0;
            inventory.AvailableChanged += _ => raisedCount++;

            bool sameGrew = inventory.UnlockUpTo(2);
            bool lowerGrew = inventory.UnlockUpTo(1);

            Assert.That(sameGrew, Is.False);
            Assert.That(lowerGrew, Is.False);
            Assert.That(raisedCount, Is.EqualTo(0));
            Assert.That(_progress.GetUnlockedPillows(), Is.EqualTo(2));
        }

        [Test]
        public void UnlockUpTo_InCapZeroLevel_PersistsButAvailableStaysZeroWithoutEvent()
        {
            var inventory = new PillowInventory(_progress, _pillowFreeLevel, _catalog);
            int raisedCount = 0;
            inventory.AvailableChanged += _ => raisedCount++;

            bool grew = inventory.UnlockUpTo(2);

            Assert.That(grew, Is.True);
            Assert.That(inventory.Available, Is.EqualTo(0));
            Assert.That(raisedCount, Is.EqualTo(0));
            Assert.That(_progress.GetUnlockedPillows(), Is.EqualTo(2));
        }

        [Test]
        public void NewInstance_ReadsPersistedCount()
        {
            new PillowInventory(_progress, _lobby, _catalog).UnlockUpTo(1);

            var reloaded = new PillowInventory(new ProgressRepository(_storage), _pillowLevel, _catalog);

            Assert.That(reloaded.Available, Is.EqualTo(1));
            Assert.That(reloaded.IsUnlocked(1), Is.True);
            Assert.That(reloaded.IsUnlocked(2), Is.False);
        }

        [Test]
        public void IsUnlocked_IgnoresLevelCap()
        {
            _progress.SetUnlockedPillowsIfHigher(2);

            var inventory = new PillowInventory(_progress, _pillowFreeLevel, _catalog);

            Assert.That(inventory.Available, Is.EqualTo(0));
            Assert.That(inventory.IsUnlocked(2), Is.True);
        }

        [Test]
        public void UnlockUpTo_BelowOne_Throws()
        {
            var inventory = new PillowInventory(_progress, _lobby, _catalog);

            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.UnlockUpTo(0));
        }

        [Test]
        public void NewInstance_KeyAbsentAndCappedLevelCompleted_SeedsHighestCompletedCap()
        {
            _progress.MarkCompleted(_lobby.LevelId);

            var inventory = new PillowInventory(_progress, _pillowLevel, _catalog);

            Assert.That(inventory.Available, Is.EqualTo(2));
            Assert.That(_progress.GetUnlockedPillows(), Is.EqualTo(2));
        }

        [Test]
        public void NewInstance_KeyAbsentAndOnlyPillowFreeLevelsCompleted_DoesNotSeed()
        {
            _progress.MarkCompleted(_pillowFreeLevel.LevelId);
            int savesBefore = _storage.SaveCount;

            var inventory = new PillowInventory(_progress, _pillowLevel, _catalog);

            Assert.That(inventory.Available, Is.EqualTo(0));
            Assert.That(_storage.HasKey(SaveKeys.UnlockedPillows), Is.False);
            Assert.That(_storage.SaveCount, Is.EqualTo(savesBefore));
        }

        [Test]
        public void NewInstance_KeyPresent_DoesNotReseed()
        {
            _progress.SetUnlockedPillowsIfHigher(1);
            _progress.MarkCompleted(_lobby.LevelId);

            var inventory = new PillowInventory(_progress, _pillowLevel, _catalog);

            Assert.That(inventory.Available, Is.EqualTo(1));
            Assert.That(_progress.GetUnlockedPillows(), Is.EqualTo(1));
        }

        private T Track<T>(T created) where T : UnityEngine.Object
        {
            _createdObjects.Add(created);
            return created;
        }
    }
}
