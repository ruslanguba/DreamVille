namespace DreamVille.Gameplay.Enemies.Presentation
{
    /// <summary>Audio seam for an enemy's footsteps and attack cue.</summary>
    public interface IEnemyAudio
    {
        void TickFootsteps(bool isMoving);

        void PlayAttack();
    }
}
