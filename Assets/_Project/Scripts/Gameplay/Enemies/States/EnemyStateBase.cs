using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>Base implementation with no-op lifecycle hooks; states override only what they need.</summary>
    public abstract class EnemyStateBase : IEnemyState
    {
        protected readonly IEnemyContext _context;
        protected readonly EnemyStateMachine _machine;

        protected EnemyStateBase(IEnemyContext context, EnemyStateMachine machine)
        {
            _context = context;
            _machine = machine;
        }

        public abstract EnemyStateId Id { get; }

        public virtual void Enter() { }

        public virtual void Tick(float deltaTime) { }

        public virtual void FixedTick(float fixedDeltaTime) { }

        public virtual void Exit() { }
    }
}
