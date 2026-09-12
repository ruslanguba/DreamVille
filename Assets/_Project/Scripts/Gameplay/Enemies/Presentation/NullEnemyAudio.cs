namespace DreamVille.Gameplay.Enemies.Presentation
{
    /// <summary>No-op audio seam for enemies without an <c>ActorAudio</c> component.</summary>
    public sealed class NullEnemyAudio : IEnemyAudio
    {
        public void TickFootsteps(bool isMoving) { }

        public void PlayAttack() { }
    }
}
