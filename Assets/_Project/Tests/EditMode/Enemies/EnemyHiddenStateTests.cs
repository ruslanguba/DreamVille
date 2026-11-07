using NUnit.Framework;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

using Object = UnityEngine.Object;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary><see cref="EnemyHiddenState"/>: the enemy stops where it arrived and hands itself to the hideout.</summary>
    public class EnemyHiddenStateTests
    {
        private EnemyConfig _config;
        private FakeEnemyContext _context;
        private EnemyStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _config = EnemyConfig.CreateForTests();
            _context = new FakeEnemyContext { Config = _config };
            _machine = new EnemyStateMachine(machine => new IEnemyState[] { new EnemyHiddenState(_context, machine) });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Enter_StopsTheBody()
        {
            _machine.Start(EnemyStateId.Hidden);

            Assert.That(_context.Steering.StopCount, Is.EqualTo(1));
            Assert.That(_context.HideoutEntered, Is.EqualTo(1));
        }
    }
}
