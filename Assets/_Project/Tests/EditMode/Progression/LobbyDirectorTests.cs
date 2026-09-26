using NUnit.Framework;

using DreamVille.Progression.Lobby;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>The lobby scenario's event table, one row per test, over a <see cref="FakeLobbyCast"/>.</summary>
    public class LobbyDirectorTests
    {
        private FakeLobbyCast _cast;
        private LobbyDirector _director;

        [SetUp]
        public void SetUp()
        {
            _cast = new FakeLobbyCast();
            _director = new LobbyDirector(_cast);
            _director.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _director.Dispose();
        }

        [Test]
        public void Initialize_DisarmsOnlyTheSecondDialogue()
        {
            Assert.That(_cast.SecondTrigger.IsArmed, Is.False);
            Assert.That(_cast.FirstTrigger.IsArmed, Is.True);
            Assert.That(_cast.GuideLog.Calls, Is.Empty);
            Assert.That(_cast.FirstGate.OpenCount, Is.EqualTo(0));
            Assert.That(_cast.SecondGate.OpenCount, Is.EqualTo(0));
        }

        [Test]
        public void FirstDialogueStarted_GuideTalks()
        {
            _cast.FirstTrigger.RaiseStarted();

            Assert.That(_cast.GuideLog.Calls, Is.EqualTo(new[] { "SetTalk" }));
        }

        [Test]
        public void FirstDialogueEnded_OpensFirstDoorAndIdlesGuide()
        {
            _cast.FirstTrigger.RaiseStarted();

            _cast.FirstTrigger.RaiseEnded();

            Assert.That(_cast.FirstGate.OpenCount, Is.EqualTo(1));
            Assert.That(_cast.SecondGate.OpenCount, Is.EqualTo(0));
            Assert.That(_cast.GuideLog.Calls, Is.EqualTo(new[] { "SetTalk", "SetIdle" }));
        }

        [Test]
        public void FirstGroupPartlyKilled_ChangesNothing()
        {
            _cast.FirstA.Die();

            Assert.That(_cast.SecondTrigger.IsArmed, Is.False);
            Assert.That(_cast.GuideLog.Calls, Is.Empty);
        }

        [Test]
        public void FirstGroupCleared_ArmsSecondDialogueAndGuideGreets()
        {
            _cast.FirstA.Die();
            _cast.FirstB.Die();

            Assert.That(_cast.SecondTrigger.IsArmed, Is.True);
            Assert.That(_cast.GuideLog.Calls, Is.EqualTo(new[] { "SetGreeting" }));
        }

        [Test]
        public void SecondDialogueStarted_GuideApplauds()
        {
            _cast.SecondTrigger.RaiseStarted();

            Assert.That(_cast.GuideLog.Calls, Is.EqualTo(new[] { "SetApplause" }));
        }

        [Test]
        public void SecondDialogueEnded_GuideIdles()
        {
            _cast.SecondTrigger.RaiseStarted();
            _cast.GuideLog.ClearCalls();

            _cast.SecondTrigger.RaiseEnded();

            Assert.That(_cast.GuideLog.Calls, Is.EqualTo(new[] { "SetIdle" }));
        }

        [Test]
        public void SecondGroupCleared_OpensSecondDoorAndIdlesGuide()
        {
            _cast.Second.Die();

            Assert.That(_cast.SecondGate.OpenCount, Is.EqualTo(1));
            Assert.That(_cast.FirstGate.OpenCount, Is.EqualTo(0));
            Assert.That(_cast.GuideLog.Calls, Is.EqualTo(new[] { "SetIdle" }));
        }

        [Test]
        public void Dispose_StopsReactingToDialoguesAndDeaths()
        {
            _director.Dispose();

            _cast.FirstTrigger.RaiseStarted();
            _cast.FirstTrigger.RaiseEnded();
            _cast.SecondTrigger.RaiseStarted();
            _cast.SecondTrigger.RaiseEnded();
            _cast.FirstA.Die();
            _cast.FirstB.Die();
            _cast.Second.Die();

            Assert.That(_cast.FirstTrigger.HandlerCount, Is.EqualTo(0));
            Assert.That(_cast.SecondTrigger.HandlerCount, Is.EqualTo(0));
            Assert.That(_cast.FirstA.HandlerCount, Is.EqualTo(0));
            Assert.That(_cast.FirstB.HandlerCount, Is.EqualTo(0));
            Assert.That(_cast.Second.HandlerCount, Is.EqualTo(0));
            Assert.That(_cast.GuideLog.Calls, Is.Empty);
            Assert.That(_cast.FirstGate.OpenCount, Is.EqualTo(0));
            Assert.That(_cast.SecondGate.OpenCount, Is.EqualTo(0));
            Assert.That(_cast.SecondTrigger.IsArmed, Is.False);
        }

        [Test]
        public void Dispose_AfterBothDialoguesDestroyedThemselves_DoesNotThrow()
        {
            _cast.FirstTriggerDestroyed = true;
            _cast.SecondTriggerDestroyed = true;

            Assert.DoesNotThrow(() => _director.Dispose());
            Assert.That(_cast.FirstA.HandlerCount, Is.EqualTo(0));
            Assert.That(_cast.Second.HandlerCount, Is.EqualTo(0));
        }
    }
}
