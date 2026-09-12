namespace DreamVille.Audio
{
    /// <summary>Sound seam for an actor (player or NPC) driven by gameplay logic instead of a concrete component.</summary>
    public interface IActorAudio
    {
        void PlayJump();
        void PlayAttack();
        void PlayDeath();

        /// <summary>Advances the footstep timer; plays a step clip when it elapses and <paramref name="isMoving"/> is true.</summary>
        void TickFootsteps(bool isMoving);
    }
}
