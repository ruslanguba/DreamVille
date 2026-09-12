namespace DreamVille.Gameplay.Enemies.Presentation
{
    /// <summary>Animator seam a state may drive without knowing the controller's declared parameters.</summary>
    public interface IEnemyAnimator
    {
        void SetMoving(bool isMoving);

        void TriggerDeath();

        void PlayLocomotion(float normalizedOffset);
    }
}
