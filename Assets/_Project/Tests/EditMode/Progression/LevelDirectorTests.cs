using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Npc;
using DreamVille.Persistence;
using DreamVille.Progression;
using DreamVille.Tests.EditMode.Fakes;

using Object = UnityEngine.Object;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>
    /// The level finale as a scenario: winning completes the level, arms the zone and greets; the final dialogue
    /// drives the NPC; its end opens the portal before the NPC reacts, so a broken NPC can never block the path on.
    /// Zone and NPC calls share one log to pin their order.
    /// </summary>
    public class LevelDirectorTests
    {
        private const string LevelId = "test";

        private List<string> _log;
        private LevelDefinition _level;
        private FakeEnemyRegistry _enemies;
        private ProgressRepository _progress;
        private FakeFinalZone _zone;
        private LoggingNpc _npc;
        private LevelDirector _director;
        private bool _isDisposedByTest;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _level = LevelDefinition.CreateForTests("Level_Test", LevelId);
            _enemies = new FakeEnemyRegistry { RemainingEnemies = 2 };
            _progress = new ProgressRepository(new InMemorySaveStorage());
            _zone = new FakeFinalZone(_log);
            _npc = new LoggingNpc(_log);
            _director = new LevelDirector(_enemies, _level, _progress, new FakeLevelCast(_zone, _npc));
        }

        [TearDown]
        public void TearDown()
        {
            if (!_isDisposedByTest)
                _director.Dispose();

            Object.DestroyImmediate(_level);
        }

        [Test]
        public void AllEnemiesKilled_MarksCompletedArmsZoneAndGreets()
        {
            _director.Initialize();

            _enemies.RaiseAllEnemiesKilled();

            Assert.That(_progress.IsCompleted(LevelId), Is.True);
            Assert.That(_log, Is.EqualTo(new[] { "Arm", "SetGreeting" }));
        }

        [Test]
        public void FinalDialogueStarted_StartsApplause()
        {
            _director.Initialize();

            _zone.RaiseDialogueStarted();

            Assert.That(_log, Is.EqualTo(new[] { "SetApplause" }));
        }

        [Test]
        public void FinalDialogueEnded_OpensPortalThenIdlesNpc()
        {
            _director.Initialize();

            _zone.RaiseDialogueEnded();

            Assert.That(_log, Is.EqualTo(new[] { "OpenPortal", "SetIdle" }));
        }

        [Test]
        public void FinalDialogueEnded_WhenTheNpcThrows_StillOpensThePortal()
        {
            _npc.ThrowOnIdle = true;
            _director.Initialize();

            Assert.Throws<InvalidOperationException>(() => _zone.RaiseDialogueEnded());

            Assert.That(_log, Does.Contain("OpenPortal"));
        }

        [Test]
        public void Initialize_WithoutEnemies_DoesNotCompleteBeforeTheFirstFrame()
        {
            _enemies.RemainingEnemies = 0;

            _director.Initialize();

            Assert.That(_progress.IsCompleted(LevelId), Is.False);
            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Dispose_ThenZoneEvents_DoNothing()
        {
            _director.Initialize();
            _director.Dispose();
            _isDisposedByTest = true;

            _enemies.RaiseAllEnemiesKilled();
            _zone.RaiseDialogueStarted();
            _zone.RaiseDialogueEnded();

            Assert.That(_progress.IsCompleted(LevelId), Is.False);
            Assert.That(_log, Is.Empty);
        }

        private sealed class LoggingNpc : INpcReactions
        {
            private readonly List<string> _log;

            public LoggingNpc(List<string> log)
            {
                _log = log;
            }

            public bool ThrowOnIdle { get; set; }

            public void SetGreeting() => _log.Add(nameof(SetGreeting));

            public void SetTalk() => _log.Add(nameof(SetTalk));

            public void SetApplause() => _log.Add(nameof(SetApplause));

            public void SetIdle()
            {
                if (ThrowOnIdle)
                    throw new InvalidOperationException("broken NPC");

                _log.Add(nameof(SetIdle));
            }
        }
    }
}
