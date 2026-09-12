using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>Terminal-until-released state: the hideout owns the transform from here.</summary>
    public sealed class EnemyHiddenState : EnemyStateBase
    {
        public EnemyHiddenState(IEnemyContext context, EnemyStateMachine machine) : base(context, machine) { }

        public override EnemyStateId Id => EnemyStateId.Hidden;

        public override void Enter()
        {
            _context.EnterHideout();
        }
    }
}
