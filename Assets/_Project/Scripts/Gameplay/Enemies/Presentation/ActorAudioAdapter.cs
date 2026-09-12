using DreamVille.Audio;

namespace DreamVille.Gameplay.Enemies.Presentation
{
    /// <summary>Adapts the existing <see cref="ActorAudio"/> component to <see cref="IEnemyAudio"/>.</summary>
    public sealed class ActorAudioAdapter : IEnemyAudio
    {
        private readonly ActorAudio _actorAudio;

        public ActorAudioAdapter(ActorAudio actorAudio)
        {
            _actorAudio = actorAudio;
        }

        public void TickFootsteps(bool isMoving) => _actorAudio.TickFootsteps(isMoving);

        public void PlayAttack() => _actorAudio.PlayAttack();
    }
}
