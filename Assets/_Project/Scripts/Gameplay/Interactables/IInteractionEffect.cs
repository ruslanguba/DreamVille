namespace DreamVille.Gameplay.Interactables
{
    /// <summary>
    /// One consequence of an interaction. Lives on the same GameObject as an <see cref="Interactable"/> or a
    /// <see cref="Pickup"/>, which applies every effect it finds there in component order.
    /// </summary>
    public interface IInteractionEffect
    {
        void Apply(in InteractionContext context);
    }
}
