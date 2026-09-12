using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>A single named state driven by <see cref="EnemyStateMachine"/>.</summary>
    public interface IEnemyState
    {
        EnemyStateId Id { get; }

        void Enter();

        void Tick(float deltaTime);

        void FixedTick(float fixedDeltaTime);

        void Exit();
    }
}
