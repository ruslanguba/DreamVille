namespace DreamVille.Gameplay.Interactables
{
    /// <summary>
    /// Something the player can shoot or touch. Senders find it with <c>TryGetComponent</c> on the hit or overlapped
    /// GameObject; receivers decide from <see cref="InteractionContext.Source"/> whether the interaction concerns them.
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(in InteractionContext context);
    }
}
