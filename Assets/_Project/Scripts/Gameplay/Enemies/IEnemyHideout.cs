using DreamVille.Gameplay.Enemies.StateMachine;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>A place an enemy can hide in. Implemented by <c>InteractableBox</c>.</summary>
    public interface IEnemyHideout
    {
        void Receive(Enemy enemy);
    }
}
