namespace DreamVille.Gameplay.Enemies
{
    /// <summary>A place an enemy can hide in. Implemented by <c>InteractableBox</c>.</summary>
    public interface IEnemyHideout
    {
        /// <summary>Takes ownership of an enemy that arrived at this hideout.</summary>
        void Receive(Enemy enemy);
    }
}
